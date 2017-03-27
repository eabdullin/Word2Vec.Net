using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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
    private const int TableSize = (int)1e8;
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
    private readonly int _window;
    private float _alpha;
    private long _fileSize;
    private int _minReduce = 1;
    private float _startingAlpha;
    private float[] _syn0;
    private float[] _syn1;
    private float[] _syn1Neg;
    private int[] _table;
    private Dictionary<string, long> _cn = new Dictionary<string, long>();
    private Dictionary<string, char[]> _code = new Dictionary<string, char[]>();
    private Dictionary<string, long> _codeLength = new Dictionary<string, long>();
    private Dictionary<string, int[]> _point = new Dictionary<string, int[]>();
    private long _wordCountActual;
    private DoubleDictionary _vocabIndex;

    internal Word2Vec(
      string trainFileName,
      string outPutfileName,
      string saveVocabFileName,
      string readVocubFileName,
      int size,
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
      _expTable = new float[ExpTableSize + 1];
      for (var i = 0; i < ExpTableSize; i++)
      {
        _expTable[i] = (float)Math.Exp((i / (float)ExpTableSize * 2 - 1) * MaxExp); // Precompute the exp() table
        _expTable[i] = _expTable[i] / (_expTable[i] + 1); // Precompute f(x) = x / (x + 1)
      }
      _readVocabFile = readVocubFileName;
      _layer1Size = size;
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


    // Create binary Huffman tree using the word counts
    // Frequent words will have short uniqe binary codes
    private void CreateBinaryTree()
    {
      var code = new char[MaxCodeLength];
      var point = new long[MaxCodeLength];
      var count = new long[_cn.Count * 2 + 1];
      var binary = new long[_cn.Count * 2 + 1];
      var parentNode = new int[_cn.Count * 2 + 1];
      var keys = _cn.Keys.ToArray();

      for (var a = 0; a < _cn.Count; a++)
        count[a] = _cn[keys[a]];
      for (var a = _cn.Count; a < _cn.Count * 2; a++)
        count[a] = (long)1e15;
      long pos1 = _cn.Count - 1;
      long pos2 = _cn.Count;
      // Following algorithm constructs the Huffman tree by adding one node at a time
      for (var a = 0; a < _cn.Count - 1; a++)
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
        count[_cn.Count + a] = count[min1I] + count[min2I];
        parentNode[min1I] = _cn.Count + a;
        parentNode[min2I] = _cn.Count + a;
        binary[min2I] = 1;
      }
      // Now assign binary code to each vocabulary word
      for (long a = 0; a < _cn.Count; a++)
      {
        var b = a;
        long i = 0;
        while (true)
        {
          code[i] = (char)binary[b];
          point[i] = b;
          i++;
          b = parentNode[b];
          if (b == _cn.Count * 2 - 2)
            break;
        }
        _codeLength[keys[a]] = (int)i;
        _point[keys[a]][0] = _cn.Count - 2;
        for (b = 0; b < i; b++)
        {
          _code[keys[a]][i - b - 1] = code[b];
          _point[keys[a]][i - b] = (int)(point[b] - _cn.Count);
        }
      }
      GC.Collect();
    }

    private void InitNet()
    {
      long a, b;
      ulong nextRandom = 1;

      _syn0 = new float[_cn.Count * _layer1Size];
      if (_hs > 0)
      {
        _syn1 = new float[_cn.Count * _layer1Size];
        for (a = 0; a < _cn.Count; a++)
          for (b = 0; b < _layer1Size; b++)
            _syn1[a * _layer1Size + b] = 0;
      }
      if (_negative > 0)
      {
        _syn1Neg = new float[_cn.Count * _layer1Size];
        for (a = 0; a < _cn.Count; a++)
          for (b = 0; b < _layer1Size; b++)
            _syn1Neg[a * _layer1Size + b] = 0;
      }
      for (a = 0; a < _cn.Count; a++)
        for (b = 0; b < _layer1Size; b++)
        {
          nextRandom = nextRandom * 25214903917 + 11;
          _syn0[a * _layer1Size + b] = ((nextRandom & 0xFFFF) / (float)65536 - (float)0.5) / _layer1Size;
        }

      CreateBinaryTree();
      GC.Collect();
    }

    private void InitUnigramTable()
    {
      if (_cn.Count == 0)
        return;

      int a;
      var power = 0.75;
      _table = new int[TableSize];
      var trainWordsPow = _cn.Sum(x => Math.Pow(x.Value, power));

      var i = 0;
      var keys = _cn.Keys.ToArray();
      var d1 = Math.Pow(_cn[keys[i]], power) / trainWordsPow;
      for (a = 0; a < TableSize; a++)
      {
        _table[a] = i;
        if (a / (double)TableSize > d1)
        {
          i++;
          d1 += Math.Pow(_cn[keys[i]], power) / trainWordsPow;
        }
        if (i >= _cn.Count)
          i = _cn.Count - 1;
      }
    }

    private void LearnVocabFromTrainFile()
    {
      if (!File.Exists(_trainFile))
        throw new InvalidOperationException("ERROR: training data file not found!\n");

      using (var fs = new FileStream(_trainFile, FileMode.Open, FileAccess.Read))
      using (var reader = new StreamReader(fs, Encoding.UTF8))
      {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
          var words = line.Split(new[] { "\r", "\n", "\t", " ", ",", "\"" }, StringSplitOptions.RemoveEmptyEntries);

          foreach (var word in words)
          {
            if (string.IsNullOrWhiteSpace(word))
              continue;

            if (_cn.ContainsKey(word))
            {
              _cn[word]++;
            }
            else
            {
              AddWordToVacab(word, 1);
            }
          }

          if (reader.EndOfStream)
            break;
        }
      }
    }

    private void ReadVocab()
    {
      using (var fin = File.OpenText(_readVocabFile))
      {
        string line;
        while ((line = fin.ReadLine()) != null)
        {
          var vals = line.Split(new[] { "\t" }, StringSplitOptions.None);
          if (vals.Length != 2)
            continue;

          AddWordToVacab(vals[0], int.Parse(vals[1]));
        }
      }
    }

    private void AddWordToVacab(string word, long cn)
    {
      _cn.Add(word, cn);
      _code.Add(word, new char[MaxCodeLength]);
      _codeLength.Add(word, 0);
      _point.Add(word, new int[MaxCodeLength]);
    }

    private void SaveVocab()
    {
      using (var stream = new FileStream(_saveVocabFile, FileMode.OpenOrCreate))
      using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
        foreach (var x in _cn)
          streamWriter.WriteLine($"{x.Key}\t{x.Value}");
    }

    /// <summary>
    ///   train model
    ///   WORD VECTOR estimation method
    /// </summary>
    public void TrainModel()
    {
      _startingAlpha = _alpha;
      _fileSize = new FileInfo(_trainFile).Length;

      if (!string.IsNullOrEmpty(_readVocabFile))
        ReadVocab();
      else
        LearnVocabFromTrainFile();

      Console.Write($"{_cn.Count} > ");
      ReduceDictionary();
      Console.WriteLine(_cn.Count);

      if (!string.IsNullOrEmpty(_saveVocabFile))
        SaveVocab();

      if (string.IsNullOrEmpty(_outputFile))
        return;
      Console.Write("InitNet...");
      InitNet();
      Console.WriteLine("ok");
      if (_negative > 0)
      {
        Console.Write("InitUnigram...");
        InitUnigramTable();
        Console.WriteLine("ok");
      }

      Console.Write("Training...");
      var parallelOptions = new ParallelOptions
      {
        MaxDegreeOfParallelism = _numThreads
      };
      _vocabIndex = new DoubleDictionary(_cn.Keys);
      var result = Parallel.For(0, _numThreads, parallelOptions, TrainModelThreadStart);
      if (!result.IsCompleted)
        throw new InvalidOperationException();
      Console.WriteLine("ok");

      Console.Write("Save...");
      using (var fs = new FileStream(_outputFile, FileMode.Create, FileAccess.Write))
      using (var writer = new StreamWriter(fs, Encoding.UTF8))
      {
        long b;
        if (_classes == 0)
        {
          writer.WriteLine(_cn.Count);
          writer.WriteLine(_layer1Size);

          var keys = _cn.Keys.ToArray();
          for (var a = 0; a < _cn.Count; a++)
          {
            var bytes = new List<byte>();
            for (b = 0; b < _layer1Size; b++)
              bytes.AddRange(BitConverter.GetBytes(_syn0[a * _layer1Size + b]));
            writer.WriteLine($"{keys[a]}\t{Convert.ToBase64String(bytes.ToArray())}");
          }
        }
        else
        {
          // Run K-means on the word vectors
          int clcn = (int)_classes, iter = 10;
          var centcn = new int[_classes];
          var cl = new int[_cn.Count];
          var cent = new float[_classes * _layer1Size];
          for (var a = 0; a < _cn.Count; a++)
            cl[a] = a % clcn;
          for (var a = 0; a < iter; a++)
          {
            for (b = 0; b < clcn * _layer1Size; b++)
              cent[b] = 0;
            for (b = 0; b < clcn; b++)
              centcn[b] = 1;
            long c;
            long d;
            for (c = 0; c < _cn.Count; c++)
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
              closev = (float)Math.Sqrt(closev);
              for (c = 0; c < _layer1Size; c++)
                cent[_layer1Size * b + c] /= closev;
            }
            for (c = 0; c < _cn.Count; c++)
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
                  closeid = (int)d;
                }
              }
              cl[c] = closeid;
            }
          }
          // Save the K-means classes
          var keys = _cn.Keys.ToArray();
          for (var a = 0; a < _cn.Count; a++)
          {
            writer.WriteLine($"{keys[a]} {cl[a]}");
          }
        }
      }
      GC.Collect();
      Console.WriteLine("ok");
    }

    private void ReduceDictionary()
    {
      var remove = (from x in _cn where x.Value < _minCount select x.Key).ToArray();
      foreach (var x in remove)
      {
        _cn.Remove(x);
        _code.Remove(x);
        _codeLength.Remove(x);
        _point.Remove(x);
      }
      GC.Collect();
    }

    private class DoubleDictionary
    {
      private Dictionary<string, long> _d1 = new Dictionary<string, long>();
      private Dictionary<long, string> _d2 = new Dictionary<long, string>();

      public DoubleDictionary(IEnumerable<string> init)
      {
        foreach (var x in init)
        {
          _d1.Add(x, _d1.Count);
          _d2.Add(_d2.Count, x);
        }
        Size = _d1.Count;
      }

      public long this[string index] => _d1.ContainsKey(index) ? _d1[index] : -1;
      public string this[long index] => _d2.ContainsKey(index) ? _d2[index] : string.Empty;
      public long Size { get; private set; }
    }

    private void TrainModelThreadStart(int id)
    {
      var splitRegex = new Regex("\\s");
      long sentenceLength = 0;
      long sentencePosition = 0;
      long wordCount = 0, lastWordCount = 0;
      var sen = new long[MaxSentenceLength + 1];
      var localIter = _iter;

      var nextRandom = (ulong)id;
      float g;
      var neu1 = new float[_layer1Size];
      var neu1E = new float[_layer1Size];
      var sum = _cn.Sum(x => x.Value);

      using (var fi = File.OpenText(_trainFile))
      {
        fi.BaseStream.Seek(_fileSize / _numThreads * id, SeekOrigin.Begin);
        while (true)
        {
          if (wordCount - lastWordCount > 10000)
          {
            _wordCountActual += wordCount - lastWordCount;
            lastWordCount = wordCount;
            _alpha = _startingAlpha * (1 - _wordCountActual / (float)(_iter * sum + 1));
            if (_alpha < _startingAlpha * (float)0.0001)
              _alpha = _startingAlpha * (float)0.0001;
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
                word = _vocabIndex[s];
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
                  var ran = ((float)Math.Sqrt(_cn[s] / (_sample * sum)) + 1) *
                            (_sample * sum) / _cn[s];
                  nextRandom = nextRandom * 25214903917 + 11;
                  if (ran < (nextRandom & 0xFFFF) / (float)65536)
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
          if (fi.EndOfStream || wordCount > sum / _numThreads)
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
          var b = (long)(nextRandom % (ulong)_window);
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
                for (d = 0; d < _codeLength[_vocabIndex[word]]; d++)
                {
                  f = 0;
                  l2 = _point[_vocabIndex[word]][d] * _layer1Size;
                  // Propagate hidden -> output
                  for (c = 0; c < _layer1Size; c++)
                    f += neu1[c] * _syn1[c + l2];
                  if (f <= MaxExp * -1)
                    continue;
                  if (f >= MaxExp)
                    continue;
                  f = _expTable[(int)((f + MaxExp) * (ExpTableSize / (float)MaxExp / 2))];
                  // 'g' is the gradient multiplied by the learning rate
                  g = (1 - _code[_vocabIndex[word]][d] - f) * _alpha;
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
                      target = (long)(nextRandom % (ulong)(_vocabIndex.Size - 1) + 1);
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
                    g = (label - _expTable[(int)((f + MaxExp) * (ExpTableSize / (float)MaxExp / 2))]) * _alpha;
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
                  for (d = 0; d < _codeLength[_vocabIndex[word]]; d++)
                  {
                    f = 0;
                    l2 = _point[_vocabIndex[word]][d] * _layer1Size;
                    // Propagate hidden -> output
                    for (c = 0; c < _layer1Size; c++)
                      f += _syn0[c + l1] * _syn1[c + l2];
                    if (f <= MaxExp * -1)
                      continue;
                    if (f >= MaxExp)
                      continue;
                    f = _expTable[(int)((f + MaxExp) * (ExpTableSize / (float)MaxExp / 2))];
                    // 'g' is the gradient multiplied by the learning rate
                    g = (1 - _code[_vocabIndex[word]][d] - f) * _alpha;
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
                        target = (long)(nextRandom % (ulong)(_vocabIndex.Size - 1) + 1);
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
                      g = (label - _expTable[(int)((f + MaxExp) * (ExpTableSize / (float)MaxExp / 2))]) *
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