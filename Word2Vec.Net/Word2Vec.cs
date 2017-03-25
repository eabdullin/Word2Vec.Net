using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Word2Vec.Net
{
  /// <summary>
  ///   the class an implementation of Google Word2Vec algorhitm
  /// </summary>
  public class Word2Vec
  {
    private const int ExpTableSize = 1000;

    private const int MaxCodeLength = 40;
    private const int MaxExp = 6;
    private const int MaxSentenceLength = 1000;
    private const int TableSize = (int) 1e8;
    private const int VocabHashSize = 30000000;
    private readonly int _binary;
    private readonly int _cbow;
    private readonly long _classes;
    private readonly float[] _expTable;
    private readonly int _hs;
    private readonly long _iter;
    private readonly int _layer1Size;
    private readonly int _minCount;
    private readonly int _negative;
    private readonly int _numThreads;
    private readonly string _outputFile;
    private readonly string _readVocabFile;
    private readonly float _sample;
    private readonly string _saveVocabFile;

    private readonly string _trainFile;
    private readonly int[] _vocabHash;
    private readonly int _window;
    private float _alpha;
    private long _fileSize;
    private int _minReduce = 1;
    private float _startingAlpha;
    private Stopwatch _stopwatch;
    private float[] _syn0;
    private float[] _syn1;
    private float[] _syn1Neg;
    private int[] _table;
    private long _trainWords;
    private VocubWord[] _vocab;
    private int _vocabMaxSize = 1000;
    private int _vocabSize;
    private long _wordCountActual;

    internal Word2Vec(
      string trainFileName,
      string outPutfileName,
      string saveVocabFileName,
      string readVocubFileName,
      int size,
      int binary,
      int cbow,
      float alpha,
      float sample,
      int hs,
      int negative,
      int threads,
      long iter,
      int minCount,
      long classes,
      int window
    )
    {
      _trainFile = trainFileName;
      _outputFile = outPutfileName;
      _saveVocabFile = saveVocabFileName;
      _vocab = new VocubWord[_vocabMaxSize];
      _vocabHash = new int[VocabHashSize];
      _expTable = new float[ExpTableSize + 1];
      for (var i = 0; i < ExpTableSize; i++)
      {
        _expTable[i] = (float) Math.Exp((i / ExpTableSize * 2 - 1) * MaxExp); // Precompute the exp() table
        _expTable[i] = _expTable[i] / (_expTable[i] + 1); // Precompute f(x) = x / (x + 1)
      }
      _readVocabFile = readVocubFileName;
      _layer1Size = size;
      _binary = binary;
      _cbow = cbow;
      _alpha = alpha;
      _sample = sample;
      _hs = hs;
      _negative = negative;
      _numThreads = threads;
      _iter = iter;
      _minCount = minCount;
      _classes = classes;
      _window = window;
    }

    /// <summary>
    ///   Adds a word to the vocabulary
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    private int AddWordToVocab(string word)
    {
      _vocab[_vocabSize].Word = word;
      _vocab[_vocabSize].Cn = 0;
      _vocabSize++;
      // Resize array if needed
      if (_vocabSize + 2 >= _vocabMaxSize)
      {
        _vocabMaxSize += 1000;
        Array.Resize(ref _vocab, _vocabMaxSize);
      }
      var hash = GetWordHash(word);
      while (_vocabHash[hash] != -1)
        hash = (hash + 1) % VocabHashSize;
      _vocabHash[hash] = _vocabSize - 1;
      return _vocabSize - 1;
    }

    // Create binary Huffman tree using the word counts
    // Frequent words will have short uniqe binary codes
    private void CreateBinaryTree()
    {
      var code = new char[MaxCodeLength];
      var point = new long[MaxCodeLength];
      var count = new long[_vocabSize * 2 + 1];
      var binary = new long[_vocabSize * 2 + 1];
      var parentNode = new int[_vocabSize * 2 + 1];

      for (var a = 0; a < _vocabSize; a++)
        count[a] = _vocab[a].Cn;
      for (var a = _vocabSize; a < _vocabSize * 2; a++)
        count[a] = (long) 1e15;
      long pos1 = _vocabSize - 1;
      long pos2 = _vocabSize;
      // Following algorithm constructs the Huffman tree by adding one node at a time
      for (var a = 0; a < _vocabSize - 1; a++)
      {
        // First, find two smallest nodes 'min1, min2'
        long min1I;
        if (pos1 >= 0)
        {
          if (count[pos1] < count[pos2])
          {
            min1I = pos1;
            pos1--;
          }
          else
          {
            min1I = pos2;
            pos2++;
          }
        }
        else
        {
          min1I = pos2;
          pos2++;
        }
        long min2I;
        if (pos1 >= 0)
        {
          if (count[pos1] < count[pos2])
          {
            min2I = pos1;
            pos1--;
          }
          else
          {
            min2I = pos2;
            pos2++;
          }
        }
        else
        {
          min2I = pos2;
          pos2++;
        }
        count[_vocabSize + a] = count[min1I] + count[min2I];
        parentNode[min1I] = _vocabSize + a;
        parentNode[min2I] = _vocabSize + a;
        binary[min2I] = 1;
      }
      // Now assign binary code to each vocabulary word
      for (long a = 0; a < _vocabSize; a++)
      {
        var b = a;
        long i = 0;
        while (true)
        {
          code[i] = (char) binary[b];
          point[i] = b;
          i++;
          b = parentNode[b];
          if (b == _vocabSize * 2 - 2)
            break;
        }
        _vocab[a].CodeLen = (int) i;
        _vocab[a].Point[0] = _vocabSize - 2;
        for (b = 0; b < i; b++)
        {
          _vocab[a].Code[i - b - 1] = code[b];
          _vocab[a].Point[i - b] = (int) (point[b] - _vocabSize);
        }
      }
      GC.Collect();
    }

    private uint GetWordHash(string word)
    {
      int a;
      ulong hash = 0;
      for (a = 0; a < word.Length; a++)
        hash = hash * 257 + word[a];
      hash = hash % VocabHashSize;
      return (uint) hash;
    }

    private void InitNet()
    {
      long a, b;
      ulong nextRandom = 1;

      _syn0 = new float[_vocabSize * _layer1Size];
      if (_hs > 0)
      {
        _syn1 = new float[_vocabSize * _layer1Size];
        for (a = 0; a < _vocabSize; a++)
        for (b = 0; b < _layer1Size; b++)
          _syn1[a * _layer1Size + b] = 0;
      }
      if (_negative > 0)
      {
        _syn1Neg = new float[_vocabSize * _layer1Size];
        for (a = 0; a < _vocabSize; a++)
        for (b = 0; b < _layer1Size; b++)
          _syn1Neg[a * _layer1Size + b] = 0;
      }
      for (a = 0; a < _vocabSize; a++)
      for (b = 0; b < _layer1Size; b++)
      {
        nextRandom = nextRandom * 25214903917 + 11;
        _syn0[a * _layer1Size + b] = ((nextRandom & 0xFFFF) / (float) 65536 - (float) 0.5) / _layer1Size;
      }

      CreateBinaryTree();
      GC.Collect();
    }

    private void InitUnigramTable()
    {
      int a;
      double trainWordsPow = 0;
      var power = 0.75;
      _table = new int[TableSize];
      for (a = 0; a < _vocabSize; a++)
        trainWordsPow += Math.Pow(_vocab[a].Cn, power);
      var i = 0;
      var d1 = Math.Pow(_vocab[i].Cn, power) / trainWordsPow;
      for (a = 0; a < TableSize; a++)
      {
        _table[a] = i;
        if (a / (double) TableSize > d1)
        {
          i++;
          d1 += Math.Pow(_vocab[i].Cn, power) / trainWordsPow;
        }
        if (i >= _vocabSize)
          i = _vocabSize - 1;
      }
    }

    private void LearnVocabFromTrainFile()
    {
      for (var a = 0; a < VocabHashSize; a++)
        _vocabHash[a] = -1;
      using (var fin = File.OpenText(_trainFile))
      {
        if (fin == StreamReader.Null)
          throw new InvalidOperationException("ERROR: training data file not found!\n");
        _vocabSize = 0;

        string line;
        var regex = new Regex("\\s");
        AddWordToVocab("</s>");
        while ((line = fin.ReadLine()) != null)
        {
          if (fin.EndOfStream)
            break;
          var words = regex.Split(line);

          foreach (var word in words)
          {
            if (string.IsNullOrWhiteSpace(word))
              continue;
            _trainWords++;
            var i = SearchVocab(word);
            if (i == -1)
            {
              var a = AddWordToVocab(word);
              _vocab[a].Cn = 1;
            }
            else
            {
              _vocab[i].Cn++;
            }
            if (_vocabSize > VocabHashSize * 0.7)
              ReduceVocab();
          }
        }
        SortVocab();
        _fileSize = new FileInfo(_trainFile).Length;
      }
    }

    private void ReadVocab()
    {
      for (var a = 0; a < VocabHashSize; a++)
        _vocabHash[a] = -1;
      _vocabSize = 0;
      using (var fin = File.OpenText(_readVocabFile))
      {
        string line;
        var regex = new Regex("\\s");
        while ((line = fin.ReadLine()) != null)
        {
          var vals = regex.Split(line);
          if (vals.Length == 2)
          {
            var a = AddWordToVocab(vals[0]);
            _vocab[a].Cn = int.Parse(vals[1]);
          }
        }
        SortVocab();
      }
      var fileInfo = new FileInfo(_trainFile);
      _fileSize = fileInfo.Length;
    }

    // Reduces the vocabulary by removing infrequent tokens
    private void ReduceVocab()
    {
      var b = 0;
      for (var a = 0; a < _vocabSize; a++)
        if (_vocab[a].Cn > _minReduce)
        {
          _vocab[b].Cn = _vocab[a].Cn;
          _vocab[b].Word = _vocab[a].Word;
          b++;
        }
        else
        {
          _vocab[a].Word = null;
        }
      _vocabSize = b;
      for (var a = 0; a < VocabHashSize; a++)
        _vocabHash[a] = -1;

      for (var a = 0; a < _vocabSize; a++)
      {
        // Hash will be re-computed, as it is not actual
        var hash = GetWordHash(_vocab[a].Word);
        while (_vocabHash[hash] != -1)
          hash = (hash + 1) % VocabHashSize;
        _vocabHash[hash] = a;
      }
      _minReduce++;
      GC.Collect();
    }

    private void SaveVocab()
    {
      using (var stream = new FileStream(_saveVocabFile, FileMode.OpenOrCreate))
      {
        using (var streamWriter = new StreamWriter(stream))
        {
          for (var i = 0; i < _vocabSize; i++)
            streamWriter.WriteLine("{0} {1}", _vocab[i].Word, _vocab[i].Cn);
        }
      }
    }

    /// <summary>
    ///   Searching the word position in the vocabulary
    /// </summary>
    /// <param name="word"></param>
    /// <returns>position of a word in the vocabulary; if the word is not found, returns -1</returns>
    private int SearchVocab(string word)
    {
      var hash = GetWordHash(word);
      while (true)
      {
        if (_vocabHash[hash] == -1)
          return -1;
        if (word.Equals(_vocab[_vocabHash[hash]].Word))
          return _vocabHash[hash];
        hash = (hash + 1) % VocabHashSize;
      }
    }


    /// <summary>
    ///   Sorts the vocabulary by frequency using word counts
    /// </summary>
    private void SortVocab()
    {
      // Sort the vocabulary and keep </s> at the first position
      Array.Sort(_vocab, 1, _vocabSize - 1, new VocubComparer());
      for (var a = 0; a < VocabHashSize; a++)
        _vocabHash[a] = -1;
      var size = _vocabSize;
      _trainWords = 0;
      for (var a = 0; a < size; a++)
        if (_vocab[a].Cn < _minCount && a != 0)
        {
          _vocabSize--;
          _vocab[a].Word = null;
        }
        else
        {
          // Hash will be re-computed, as after the sorting it is not actual
          var hash = GetWordHash(_vocab[a].Word);
          while (_vocabHash[hash] != -1)
            hash = (hash + 1) % VocabHashSize;
          _vocabHash[hash] = a;
          _trainWords += _vocab[a].Cn;
        }
      Array.Resize(ref _vocab, _vocabSize + 1);

      // Allocate memory for the binary tree construction
      for (var a = 0; a < _vocabSize; a++)
      {
        _vocab[a].Code = new char[MaxCodeLength];
        _vocab[a].Point = new int[MaxCodeLength];
      }
      GC.Collect();
    }

    /// <summary>
    ///   train model
    ///   WORD VECTOR estimation method
    /// </summary>
    public void TrainModel()
    {
      _startingAlpha = _alpha;
      if (!string.IsNullOrEmpty(_readVocabFile))
        ReadVocab();
      else
        LearnVocabFromTrainFile();
      if (!string.IsNullOrEmpty(_saveVocabFile))
        SaveVocab();
      if (string.IsNullOrEmpty(_outputFile))
        return;
      InitNet();
      if (_negative > 0)
        InitUnigramTable();
      _stopwatch = new Stopwatch();
      _stopwatch.Start();
      var parallelOptions = new ParallelOptions
      {
        MaxDegreeOfParallelism = _numThreads
      };
      var result = Parallel.For(0, _numThreads, parallelOptions, TrainModelThreadStart);
      if (!result.IsCompleted)
        throw new InvalidOperationException();
      
      using (var stream = new FileStream(_outputFile, FileMode.Create, FileAccess.Write))
      {
        long b;
        if (_classes == 0)
        {
          // Save the word vectors
          var bytes = $"{_vocabSize} {_layer1Size}\n".GetBytes();
          stream.Write(bytes, 0, bytes.Length);
          for (var a = 0; a < _vocabSize; a++)
          {
            bytes = string.Concat(_vocab[a].Word, ' ').GetBytes();
            stream.Write(bytes, 0, bytes.Length);
            if (_binary > 0)
              for (b = 0; b < _layer1Size; b++)
              {
                bytes = BitConverter.GetBytes(_syn0[a * _layer1Size + b]);
                stream.Write(bytes, 0, bytes.Length);
              }

            else
              for (b = 0; b < _layer1Size; b++)
              {
                bytes = string.Concat(_syn0[a * _layer1Size + b], " ").GetBytes();
                stream.Write(bytes, 0, bytes.Length);
              }
            bytes = "\n".GetBytes();
            stream.Write(bytes, 0, bytes.Length);
          }
        }
        else
        {
          // Run K-means on the word vectors
          int clcn = (int) _classes, iter = 10;
          var centcn = new int[_classes];
          var cl = new int[_vocabSize];
          var cent = new float[_classes * _layer1Size];
          for (var a = 0; a < _vocabSize; a++)
            cl[a] = a % clcn;
          for (var a = 0; a < iter; a++)
          {
            for (b = 0; b < clcn * _layer1Size; b++)
              cent[b] = 0;
            for (b = 0; b < clcn; b++)
              centcn[b] = 1;
            long c;
            long d;
            for (c = 0; c < _vocabSize; c++)
            {
              for (d = 0; d < _layer1Size; d++)
                cent[_layer1Size * cl[c] + d] += _syn0[c * _layer1Size + d];
              centcn[cl[c]]++;
            }
            float closev;
            for (b = 0; b < clcn; b++)
            {
              closev = 0;
              for (c = 0; c < _layer1Size; c++)
              {
                cent[_layer1Size * b + c] /= centcn[b];
                closev += cent[_layer1Size * b + c] * cent[_layer1Size * b + c];
              }
              closev = (float) Math.Sqrt(closev);
              for (c = 0; c < _layer1Size; c++)
                cent[_layer1Size * b + c] /= closev;
            }
            for (c = 0; c < _vocabSize; c++)
            {
              closev = -10;
              var closeid = 0;
              for (d = 0; d < clcn; d++)
              {
                float x = 0;
                for (b = 0; b < _layer1Size; b++)
                  x += cent[_layer1Size * d + b] * _syn0[c * _layer1Size + b];
                if (x > closev)
                {
                  closev = x;
                  closeid = (int) d;
                }
              }
              cl[c] = closeid;
            }
          }
          // Save the K-means classes
          for (var a = 0; a < _vocabSize; a++)
          {
            var bytes = $"{_vocab[a].Word} {cl[a]}\n".GetBytes();
            stream.Write(bytes, 0, bytes.Length);
          }
        }
      }
      GC.Collect();
    }

    private void TrainModelThreadStart(int id)
    {
      var splitRegex = new Regex("\\s");
      long sentenceLength = 0;
      long sentencePosition = 0;
      long wordCount = 0, lastWordCount = 0;
      var sen = new long[MaxSentenceLength + 1];
      var localIter = _iter;

      var nextRandom = (ulong) id;
      float g;
      var neu1 = new float[_layer1Size];
      var neu1E = new float[_layer1Size];
      using (var fi = File.OpenText(_trainFile))
      {
        fi.BaseStream.Seek(_fileSize / _numThreads * id, SeekOrigin.Begin);
        while (true)
        {
          if (wordCount - lastWordCount > 10000)
          {
            _wordCountActual += wordCount - lastWordCount;
            lastWordCount = wordCount;
            _alpha = _startingAlpha * (1 - _wordCountActual / (float) (_iter * _trainWords + 1));
            if (_alpha < _startingAlpha * (float) 0.0001)
              _alpha = _startingAlpha * (float) 0.0001;
          }
          long word;
          if (sentenceLength == 0)
          {
            string line;
            var loopEnd = false;
            while (!loopEnd && (line = fi.ReadLine()) != null)
            {
              var words = splitRegex.Split(line);
              foreach (var s in words)
              {
                word = SearchVocab(s);
                if (fi.EndOfStream)
                {
                  loopEnd = true;
                  break;
                }
                if (word == -1)
                  continue;
                wordCount++;
                if (word == 0)
                {
                  loopEnd = true;
                  break;
                }
                // The subsampling randomly discards frequent words while keeping the ranking same
                if (_sample > 0)
                {
                  var ran = ((float) Math.Sqrt(_vocab[word].Cn / (_sample * _trainWords)) + 1) *
                            (_sample * _trainWords) / _vocab[word].Cn;
                  nextRandom = nextRandom * 25214903917 + 11;
                  if (ran < (nextRandom & 0xFFFF) / (float) 65536)
                    continue;
                }
                sen[sentenceLength] = word;
                sentenceLength++;
                if (sentenceLength >= MaxSentenceLength)
                {
                  loopEnd = true;
                  break;
                }
              }
            }
            sentencePosition = 0;
          }
          if (fi.EndOfStream || wordCount > _trainWords / _numThreads)
          {
            _wordCountActual += wordCount - lastWordCount;
            localIter--;
            if (localIter == 0)
              break;
            wordCount = 0;
            lastWordCount = 0;
            sentenceLength = 0;
            fi.BaseStream.Seek(_fileSize / _numThreads * id, SeekOrigin.Begin);
            continue;
          }
          word = sen[sentencePosition];
          if (word == -1)
            continue;
          long c;
          for (c = 0; c < _layer1Size; c++)
            neu1[c] = 0;
          for (c = 0; c < _layer1Size; c++)
            neu1E[c] = 0;
          nextRandom = nextRandom * 25214903917 + 11;
          var b = (long) (nextRandom % (ulong) _window);
          long label;
          long lastWord;
          long d;
          float f;
          long target;
          long l2;
          if (_cbow > 0)
          {
            //train the cbow architecture
            // in -> hidden
            long cw = 0;
            for (var a = b; a < _window * 2 + 1 - b; a++)
              if (a != _window)
              {
                c = sentencePosition - _window + a;
                if (c < 0)
                  continue;
                if (c >= sentenceLength)
                  continue;
                lastWord = sen[c];
                if (lastWord == -1)
                  continue;
                for (c = 0; c < _layer1Size; c++)
                  neu1[c] += _syn0[c + lastWord * _layer1Size];
                cw++;
              }
            if (cw > 0)
            {
              for (c = 0; c < _layer1Size; c++)
                neu1[c] /= cw;
              if (_hs > 0)
                for (d = 0; d < _vocab[word].CodeLen; d++)
                {
                  f = 0;
                  l2 = _vocab[word].Point[d] * _layer1Size;
                  // Propagate hidden -> output
                  for (c = 0; c < _layer1Size; c++)
                    f += neu1[c] * _syn1[c + l2];
                  if (f <= MaxExp * -1)
                    continue;
                  if (f >= MaxExp)
                    continue;
                  f = _expTable[(int) ((f + MaxExp) * (ExpTableSize / MaxExp / 2))];
                  // 'g' is the gradient multiplied by the learning rate
                  g = (1 - _vocab[word].Code[d] - f) * _alpha;
                  // Propagate errors output -> hidden
                  for (c = 0; c < _layer1Size; c++)
                    neu1E[c] += g * _syn1[c + l2];
                  // Learn weights hidden -> output
                  for (c = 0; c < _layer1Size; c++)
                    _syn1[c + l2] += g * neu1[c];
                }
              // NEGATIVE SAMPLING
              if (_negative > 0)
                for (d = 0; d < _negative + 1; d++)
                {
                  if (d == 0)
                  {
                    target = word;
                    label = 1;
                  }
                  else
                  {
                    nextRandom = nextRandom * 25214903917 + 11;
                    target = _table[(nextRandom >> 16) % TableSize];
                    if (target == 0)
                      target = (long) (nextRandom % (ulong) (_vocabSize - 1) + 1);
                    if (target == word)
                      continue;
                    label = 0;
                  }
                  l2 = target * _layer1Size;
                  f = 0;
                  for (c = 0; c < _layer1Size; c++)
                    f += neu1[c] * _syn1Neg[c + l2];
                  if (f > MaxExp)
                    g = (label - 1) * _alpha;
                  else if (f < MaxExp * -1)
                    g = (label - 0) * _alpha;
                  else
                    g = (label - _expTable[(int) ((f + MaxExp) * (ExpTableSize / MaxExp / 2))]) * _alpha;
                  for (c = 0; c < _layer1Size; c++)
                    neu1E[c] += g * _syn1Neg[c + l2];
                  for (c = 0; c < _layer1Size; c++)
                    _syn1Neg[c + l2] += g * neu1[c];
                }
              // hidden -> in
              for (var a = b; a < _window * 2 + 1 - b; a++)
                if (a != _window)
                {
                  c = sentencePosition - _window + a;
                  if (c < 0)
                    continue;
                  if (c >= sentenceLength)
                    continue;
                  lastWord = sen[c];
                  if (lastWord == -1)
                    continue;
                  for (c = 0; c < _layer1Size; c++)
                    _syn0[c + lastWord * _layer1Size] += neu1E[c];
                }
            }
          }
          else
          {
            //train skip-gram
            for (var a = b; a < _window * 2 + 1 - b; a++)
              if (a != _window)
              {
                c = sentencePosition - _window + a;
                if (c < 0)
                  continue;
                if (c >= sentenceLength)
                  continue;
                lastWord = sen[c];
                if (lastWord == -1)
                  continue;
                var l1 = lastWord * _layer1Size;
                for (c = 0; c < _layer1Size; c++)
                  neu1E[c] = 0;
                // HIERARCHICAL SOFTMAX
                if (_hs != 0)
                  for (d = 0; d < _vocab[word].CodeLen; d++)
                  {
                    f = 0;
                    l2 = _vocab[word].Point[d] * _layer1Size;
                    // Propagate hidden -> output
                    for (c = 0; c < _layer1Size; c++)
                      f += _syn0[c + l1] * _syn1[c + l2];
                    if (f <= MaxExp * -1)
                      continue;
                    if (f >= MaxExp)
                      continue;
                    f = _expTable[(int) ((f + MaxExp) * (ExpTableSize / MaxExp / 2))];
                    // 'g' is the gradient multiplied by the learning rate
                    g = (1 - _vocab[word].Code[d] - f) * _alpha;
                    // Propagate errors output -> hidden
                    for (c = 0; c < _layer1Size; c++)
                      neu1E[c] += g * _syn1[c + l2];
                    // Learn weights hidden -> output
                    for (c = 0; c < _layer1Size; c++)
                      _syn1[c + l2] += g * _syn0[c + l1];
                  }
                // NEGATIVE SAMPLING
                if (_negative > 0)
                  for (d = 0; d < _negative + 1; d++)
                  {
                    if (d == 0)
                    {
                      target = word;
                      label = 1;
                    }
                    else
                    {
                      nextRandom = nextRandom * 25214903917 + 11;
                      target = _table[(nextRandom >> 16) % TableSize];
                      if (target == 0)
                        target = (long) (nextRandom % (ulong) (_vocabSize - 1) + 1);
                      if (target == word)
                        continue;
                      label = 0;
                    }
                    l2 = target * _layer1Size;
                    f = 0;
                    for (c = 0; c < _layer1Size; c++)
                      f += _syn0[c + l1] * _syn1Neg[c + l2];
                    if (f > MaxExp)
                      g = (label - 1) * _alpha;
                    else if (f < MaxExp * -1)
                      g = (label - 0) * _alpha;
                    else
                      g = (label - _expTable[(int) ((f + MaxExp) * (ExpTableSize / MaxExp / 2))]) *
                          _alpha;
                    for (c = 0; c < _layer1Size; c++)
                      neu1E[c] += g * _syn1Neg[c + l2];
                    for (c = 0; c < _layer1Size; c++)
                      _syn1Neg[c + l2] += g * _syn0[c + l1];
                  }
                // Learn weights input -> hidden
                for (c = 0; c < _layer1Size; c++)
                  _syn0[c + l1] += neu1E[c];
              }
          }
          sentencePosition++;
          if (sentencePosition >= sentenceLength)
            sentenceLength = 0;
        }
      }
      GC.Collect();
    }
  }
}