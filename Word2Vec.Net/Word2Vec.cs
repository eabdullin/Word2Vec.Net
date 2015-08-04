using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Word2Vec.Net
{
    public class Word2Vec
    {
        private const int MAX_CODE_LENGTH = 40;
        private const int MAX_SENTENCE_LENGTH = 1000;
        private const int MAX_STRING = 100;
        private const int EXP_TABLE_SIZE = 1000;
        private const int MAX_EXP = 6;

        private const int VocabHashSize = 30000000;
        private readonly string _trainFile;
        private readonly string _outputFile;
        private readonly string _saveVocabFile;
        private readonly string _readVocabFile;
        private VocubWord[] _vocab;
        private readonly int _binary;
        private readonly int _cbow;
        private readonly int _debugMode;
        private readonly int _window = 5;
        private readonly int _minCount;
        private readonly int _numThreads;
        private int _minReduce = 1;
        private readonly int[] _vocabHash;
        private long _vocabMaxSize = 1000;
        private int _vocabSize;
        private readonly int _layer1Size;
        private long _trainWords;
        private long _wordCountActual = 0;
        private readonly long _iter;
        private long _fileSize;
        private readonly long _classes;
        private double _alpha;
        private double _startingAlpha;
        private readonly double _sample;
        private double[] _syn0;
        private double[] _syn1;
        private double[] _syn1Neg;
        private readonly double[] _expTable;
        private DateTime _start;
        private readonly int _hs;
        private readonly int _negative;
        private const double TableSize = 1e8;
        private int[] _table;

        internal Word2Vec(
            string trainFileName,
            string outPutfileName,
            string saveVocabFileName,
            string readVocubFileName,
            int size,
            int debugMode,
            int binary,
            int cbow,
            double alpha,
            double sample,
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
            _expTable = new double[EXP_TABLE_SIZE + 1];
            for (int i = 0; i < EXP_TABLE_SIZE; i++)
            {
                _expTable[i] = Math.Exp((i / (double)EXP_TABLE_SIZE * 2 - 1) * MAX_EXP); // Precompute the exp() table
                _expTable[i] = _expTable[i] / (_expTable[i] + 1);                   // Precompute f(x) = x / (x + 1)
            }
            _readVocabFile = readVocubFileName;
            _layer1Size = size;
            _debugMode = debugMode;
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
        private void InitUnigramTable()
        {
            int a, i;
            double train_words_pow = 0;
            double d1, power = 0.75;
            _table = new int[(int) TableSize];
            for (a = 0; a < _vocabSize; a++) train_words_pow += Math.Pow(_vocab[a].Cn, power);
            i = 0;
            d1 = Math.Pow(_vocab[i].Cn, power)/train_words_pow;
            for (a = 0; a < TableSize; a++)
            {
                _table[a] = i;
                if (a/TableSize > d1)
                {
                    i++;
                    d1 += Math.Pow(_vocab[i].Cn, power)/train_words_pow;
                }
                if (i >= _vocabSize) i = _vocabSize - 1;
            }
        }

        // Reads a single word from a file, assuming space + tab + EOL to be word boundaries
        private string ReadWord(StreamReader fin)
        {
            var stringBuilder = new StringBuilder();
            int a = 0;
            char ch;
            while (!fin.EndOfStream)
            {
                try
                {

                    ch = (char)fin.Read();
                }
                catch (EndOfStreamException e)
                {
                    break;
                }
                
                if (ch == 13) continue;
                if ((ch == ' ') || (ch == '\t') || (ch == '\n'))
                {
                    if (a > 0)
                    {
                        if (ch == '\n') fin.Peek();
                        break;
                    }
                    if (ch == '\n')
                    {
                        return "</s>";
                    }
                    continue;
                }
                stringBuilder.Append(ch);
                a++;
            }
            return stringBuilder.ToString();
        }

        // Returns hash value of a word
        private int GetWordHash(string word)
        {
            int a;
            ulong hash = 0;
            for (a = 0; a < word.Length; a++)
            {
                hash = hash*257 + word[a];
            }
            hash = hash%VocabHashSize;
            return (int)hash;
        }

        // Returns position of a word in the vocabulary; if the word is not found, returns -1
        private int SearchVocab(string word)
        {
            var hash = (uint) GetWordHash(word);
            while (true)
            {
                if (_vocabHash[hash] == -1) return -1;
                if (!word.Equals(_vocab[_vocabHash[hash]].Word)) return _vocabHash[hash];
                hash = (hash + 1)%VocabHashSize;
            }
            return -1;
        }


        // Reads a word and returns its index in the vocabulary
        private int ReadWordIndex(StreamReader fin)
        {
            var word = ReadWord(fin);
            if (fin.EndOfStream) return -1;
            return SearchVocab(word);
        }

        // Adds a word to the vocabulary
        protected int AddWordToVocab(string word)
        {
            int hash;
            if (_vocab[_vocabSize] == null) _vocab[_vocabSize] = new VocubWord();
            _vocab[_vocabSize].Word = word;
            _vocab[_vocabSize].Cn = 0;
            _vocabSize++;
            // Reallocate memory if needed
            if (_vocabSize + 2 >= _vocabMaxSize)
            {
                _vocabMaxSize += 1000;
                Array.Resize(ref _vocab, (int) _vocabMaxSize);
            }
            hash = GetWordHash(word);
            while (_vocabHash[hash] != -1) hash = (hash + 1)%VocabHashSize;
            _vocabHash[hash] = _vocabSize - 1;
            return _vocabSize - 1;
        }

        // Sorts the vocabulary by frequency using word counts
        private void SortVocab()
        {
            int a;
            int size;
            int hash;
            // Sort the vocabulary and keep </s> at the first position
            Array.Sort(_vocab, new VocubComparer());
            //qsort(&vocab[1], vocab_size - 1, sizeof(struct vocab_word), VocabCompare);

            for (a = 0; a < VocabHashSize; a++) _vocabHash[a] = -1;
            size = _vocabSize;
            _trainWords = 0;
            for (a = 0; a < size; a++)
            {
                // Words occuring less than min_count times will be discarded from the vocab
                if ((_vocab[a] == null || _vocab[a].Cn < _minCount) && (a != 0))
                {
                    _vocabSize--;
                    if(_vocab[a] != null)
                        _vocab[a].Word = null;
                }
                else
                {
                    // Hash will be re-computed, as after the sorting it is not actual
                    hash = GetWordHash(_vocab[a].Word);
                    while (_vocabHash[hash] != -1) hash = (hash + 1)%VocabHashSize;
                    _vocabHash[hash] = a;
                    _trainWords += _vocab[a].Cn;
                }
            }
            Array.Resize(ref _vocab, _vocabSize + 1);
            // Allocate memory for the binary tree construction
            for (a = 0; a < _vocabSize; a++)
            {
                _vocab[a].Code = new char[MAX_CODE_LENGTH];
                _vocab[a].Point = new int[MAX_CODE_LENGTH];
            }
        }

        // Reduces the vocabulary by removing infrequent tokens
        private void ReduceVocab()
        {
            int a, b = 0;
            int hash;
            for (a = 0; a < _vocabSize; a++)
                if (_vocab[a].Cn > _minReduce)
                {
                    _vocab[b].Cn = _vocab[a].Cn;
                    _vocab[b].Word = _vocab[a].Word;
                    b++;
                }
                else _vocab[a].Word = null;
            _vocabSize = b;
            for (a = 0; a < VocabHashSize; a++) _vocabHash[a] = -1;
            for (a = 0; a < _vocabSize; a++)
            {
                // Hash will be re-computed, as it is not actual
                hash = GetWordHash(_vocab[a].Word);
                while (_vocabHash[hash] != -1) hash = (hash + 1)%VocabHashSize;
                _vocabHash[hash] = a;
            }
            //fflush(stdout);
            _minReduce++;
        }

        // Create binary Huffman tree using the word counts
        // Frequent words will have short uniqe binary codes
        private void CreateBinaryTree()
        {
            int a, b, i, min1i, min2i, pos1, pos2;
            var code = new char[MAX_CODE_LENGTH];
            var point = new long[MAX_CODE_LENGTH];
            var count = new long[_vocabSize*2 + 1];
            var binary = new long[_vocabSize*2 + 1];
            var parent_node = new int[_vocabSize*2 + 1];

            var d = 1e15;
            for (a = 0; a < _vocabSize; a++) count[a] = _vocab[a].Cn;
            for (a = _vocabSize; a < _vocabSize*2; a++) count[a] = (long) d;
            pos1 = _vocabSize - 1;
            pos2 = _vocabSize;
            // Following algorithm constructs the Huffman tree by adding one node at a time
            for (a = 0; a < _vocabSize - 1; a++)
            {
                // First, find two smallest nodes 'min1, min2'
                if (pos1 >= 0)
                {
                    if (count[pos1] < count[pos2])
                    {
                        min1i = pos1;
                        pos1--;
                    }
                    else
                    {
                        min1i = pos2;
                        pos2++;
                    }
                }
                else
                {
                    min1i = pos2;
                    pos2++;
                }
                if (pos1 >= 0)
                {
                    if (count[pos1] < count[pos2])
                    {
                        min2i = pos1;
                        pos1--;
                    }
                    else
                    {
                        min2i = pos2;
                        pos2++;
                    }
                }
                else
                {
                    min2i = pos2;
                    pos2++;
                }
                count[_vocabSize + a] = count[min1i] + count[min2i];
                parent_node[min1i] = _vocabSize + a;
                parent_node[min2i] = _vocabSize + a;
                binary[min2i] = 1;
            }
            // Now assign binary code to each vocabulary word
            for (a = 0; a < _vocabSize; a++)
            {
                b = a;
                i = 0;
                while (true)
                {
                    code[i] = (char) binary[b];
                    point[i] = b;
                    i++;
                    b = parent_node[b];
                    if (b == _vocabSize*2 - 2) break;
                }
                _vocab[a].CodeLen = (char) i;
                _vocab[a].Point[0] = _vocabSize - 2;
                for (b = 0; b < i; b++)
                {
                    _vocab[a].Code[i - b - 1] = code[b];
                    _vocab[a].Point[i - b] = (int) (point[b] - _vocabSize);
                }
            }
        }

        private void LearnVocabFromTrainFile()
        {
            using (var stream = new FileStream(_trainFile, FileMode.Open))
            {
                int a, i;
                for (a = 0; a < VocabHashSize; a++) _vocabHash[a] = -1;
                using (var fin = new StreamReader(stream))
                {
                    if (fin == StreamReader.Null)
                    {
                        Console.WriteLine("ERROR: training data file not found!\n");
                        throw new InvalidOperationException("ERROR: training data file not found!\n");;
                    }
                    _vocabSize = 0;
                    AddWordToVocab("</s>");
                    while (true)
                    {
                        var word = ReadWord(fin);
                        if (fin.EndOfStream) break;
                        _trainWords++;
                        if ((_debugMode > 1) && (_trainWords%100000 == 0))
                        {
                            Console.WriteLine("{0,1}", _trainWords/1000, 13);
                            //printf("%lldK%c", train_words / 1000, 13);
                            //fflush(stdout);
                        }
                        i = SearchVocab(word);
                        if (i == -1)
                        {
                            a = AddWordToVocab(word);
                            _vocab[a].Cn = 1;
                        }
                        else _vocab[i].Cn++;
                        if (_vocabSize > VocabHashSize*0.7) 
                            ReduceVocab();
                    }
                    SortVocab();
                    if (_debugMode > 0)
                    {
                        Console.WriteLine("Vocab size: {0}", _vocabSize);
                        Console.WriteLine("Words in train file: {0}", _trainWords);
                    }
                    //file_size = ftell(fin);
                    _fileSize = new FileInfo(_trainFile).Length;
                }
            }
        }

        private void SaveVocab()
        {
            using (var stream = new FileStream(_saveVocabFile, FileMode.OpenOrCreate))
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    for (var i = 0; i < _vocabSize; i++) streamWriter.WriteLine("{0} {1}", _vocab[i].Word, _vocab[i].Cn);
                }
            }
        }
        
        void ReadVocab() {
          int a, i = 0;
          char c;
            string word;
            using (var stream = new FileStream(_readVocabFile, FileMode.Open))
            {
                for (a = 0; a < VocabHashSize; a++) _vocabHash[a] = -1;
                using (var fin = new StreamReader(stream))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {


                        for (a = 0; a < VocabHashSize; a++) _vocabHash[a] = -1;
                        _vocabSize = 0;
                        while (true)
                        {
                            word = ReadWord(fin);
                            if (!stream.CanRead) break;
                            a = AddWordToVocab(word);
                            _vocab[a].Cn = reader.ReadInt32();
                            //fscanf(fin, "%lld%c", &, &c);
                            i++;
                        }
                        SortVocab();
                        if (_debugMode > 0)
                        {
                            Console.WriteLine("Vocab size: {0}", _vocabSize);
                            Console.WriteLine("Words in train file: {0}", _trainWords);
                        }
                    }
                }
            }
            FileInfo fileInfo = new FileInfo(_trainFile);
            _fileSize = fileInfo.Length;
            //using (FileStream stream = new FileStream(train_file,FileMode.Open))
            //{
            //    fin = fopen(train_file, "rb");
            //    if (fin == NULL)
            //    {
            //        printf("ERROR: training data file not found!\n");
            //        exit(1);
            //    }

            //    fseek(fin, 0, SEEK_END);
            //    file_size = ftell(fin);
            //    fclose(fin);
 
                
            //}
        }
        void InitNet() {
          long a, b;
          ulong next_random = 1;
            _syn0 = new double[_vocabSize * _layer1Size ];
          //a = posix_memalign((void **)&syn0, 128, (long long)vocab_size * layer1_size * sizeof(real));
            if (_syn0 == null)
            {
                Console.WriteLine("Memory allocation failed");
                throw new InvalidOperationException("Memory allocation failed");
            }
          if (_hs > 0) {
            //a = posix_memalign((void **)&syn1, 128, (long long)vocab_size * layer1_size * sizeof(real));
               _syn1 = new double[_vocabSize * _layer1Size ];
              if (_syn1 == null)
              {
                  //printf("Memory allocation failed\n"); exit(1);
                                  Console.WriteLine("Memory allocation failed");
                throw new InvalidOperationException("Memory allocation failed");
              }
            for (a = 0; a < _vocabSize; a++) for (b = 0; b < _layer1Size; b++)
             _syn1[a * _layer1Size + b] = 0;
          }
          if (_negative>0) {
            //a = posix_memalign((void **)&syn1neg, 128, (long long)vocab_size * layer1_size * sizeof(real));
              _syn1Neg = new double[_vocabSize * _layer1Size ];
              if (_syn1Neg == null)
              {
                  //printf("Memory allocation failed\n"); exit(1);
                  Console.WriteLine("Memory allocation failed");
                  throw new InvalidOperationException("Memory allocation failed");
              }
            for (a = 0; a < _vocabSize; a++) for (b = 0; b < _layer1Size; b++)
             _syn1Neg[a * _layer1Size + b] = 0;
          }
          for (a = 0; a < _vocabSize; a++) for (b = 0; b < _layer1Size; b++) {
            next_random = next_random * (long)25214903917 + 11;
            _syn0[a * _layer1Size + b] = (((next_random & 0xFFFF) / (double)65536) - 0.5) / _layer1Size;
          }
          CreateBinaryTree();
        }

        private void TrainModelThreadStart(object idObject)
        {
            long a, b, d, cw, word, last_word, sentence_length = 0, sentence_position = 0;
            long word_count = 0, last_word_count = 0;
            var sen = new long[MAX_SENTENCE_LENGTH + 1];
            long l1, l2, c, target, label, local_iter = _iter;
            int id = (Int32) idObject;
            long next_random = id;
            double f, g;
            DateTime now;
            var neu1 = new double[_layer1Size];
            var neu1e = new double[_layer1Size];
            //FILE *fi = fopen(train_file, "rb");
            using (var stream = File.Open(_trainFile,FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var fi = new StreamReader(stream))
                {
                    stream.Seek(_fileSize/_numThreads*id, SeekOrigin.Begin);
                    while (true)
                    {
                        if (word_count - last_word_count > 10000)
                        {
                            _wordCountActual += word_count - last_word_count;
                            last_word_count = word_count;
                            if ((_debugMode > 1))
                            {
                                now = DateTime.Now;
                                Console.WriteLine("{0}Alpha: 1  Progress: {2}  Words/thread/sec:{3}  ",13,_alpha,_wordCountActual/(double) (_iter*_trainWords + 1)*100,_wordCountActual/((double) (now - _start).Ticks/(double) TimeSpan.TicksPerSecond*1000));
                                //printf("%cAlpha: %f  Progress: %.2f%%  Words/thread/sec: %.2fk  ", 13, alpha,
                                //    word_count_actual/(real) (iter*train_words + 1)*100,
                                //    word_count_actual/((real) (now - start + 1)/(real) CLOCKS_PER_SEC*1000));
                                //fflush(stdout);
                            }
                            _alpha = _startingAlpha*(1 - _wordCountActual/(double) (_iter*_trainWords + 1));
                            if (_alpha < _startingAlpha*0.0001) _alpha = _startingAlpha*0.0001;
                        }
                        if (sentence_length == 0)
                        {
                            while (true)
                            {
                                word = ReadWordIndex(fi);
                                if (fi.EndOfStream) break;
                                if (word == -1) continue;
                                word_count++;
                                if (word == 0) break;
                                // The subsampling randomly discards frequent words while keeping the ranking same
                                if (_sample > 0)
                                {
                                    double ran = (Math.Sqrt(_vocab[word].Cn/(_sample*_trainWords)) + 1)*(_sample*_trainWords)/
                                               _vocab[word].Cn;
                                    next_random = next_random*((long)25214903917 + 11);
                                    if (ran < (next_random & 0xFFFF)/(double) 65536) continue;
                                }
                                sen[sentence_length] = word;
                                sentence_length++;
                                if (sentence_length >= MAX_SENTENCE_LENGTH) break;
                            }
                            sentence_position = 0;
                        }
                        if (fi.EndOfStream || (word_count > _trainWords/_numThreads))
                        {
                            _wordCountActual += word_count - last_word_count;
                            local_iter--;
                            if (local_iter == 0) break;
                            word_count = 0;
                            last_word_count = 0;
                            sentence_length = 0;
                            stream.Seek(_fileSize/(long) _numThreads*(long) id, SeekOrigin.Begin);
                            //fseek(fi, file_size/(long long ) num_threads*(long long ) id, SEEK_SET)
                            //;
                            continue;
                        }
                        word = sen[sentence_position];
                        if (word == -1) continue;
                        for (c = 0; c < _layer1Size; c++) neu1[c] = 0;
                        for (c = 0; c < _layer1Size; c++) neu1e[c] = 0;
                        next_random = next_random*((long )25214903917 + 11);
                        b = next_random%_window;
                        if (_cbow > 0)
                        {
                            //train the cbow architecture
                            // in -> hidden
                            cw = 0;
                            for (a = b; a < _window*2 + 1 - b; a++)
                                if (a != _window)
                                {
                                    c = sentence_position - _window + a;
                                    if (c < 0) continue;
                                    if (c >= sentence_length) continue;
                                    last_word = sen[c];
                                    if (last_word == -1) continue;
                                    for (c = 0; c < _layer1Size; c++) neu1[c] += _syn0[c + last_word*_layer1Size];
                                    cw++;
                                }
                            if (cw!=0)
                            {
                                for (c = 0; c < _layer1Size; c++) neu1[c] /= cw;
                                if (_hs!=0)
                                    for (d = 0; d < _vocab[word].CodeLen; d++)
                                    {
                                        f = 0;
                                        l2 = _vocab[word].Point[d]*_layer1Size;
                                        // Propagate hidden -> output
                                        for (c = 0; c < _layer1Size; c++) f += neu1[c]*_syn1[c + l2];
                                        if (f <= -MAX_EXP) continue;
                                        if (f >= MAX_EXP) continue;
                                        f = _expTable[(int) ((f + MAX_EXP)*(EXP_TABLE_SIZE/MAX_EXP/2))];
                                        // 'g' is the gradient multiplied by the learning rate
                                        g = (1 - _vocab[word].Code[d] - f)*_alpha;
                                        // Propagate errors output -> hidden
                                        for (c = 0; c < _layer1Size; c++) neu1e[c] += g*_syn1[c + l2];
                                        // Learn weights hidden -> output
                                        for (c = 0; c < _layer1Size; c++) _syn1[c + l2] += g*neu1[c];
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
                                            //next_random = next_random*(unsigned long long )25214903917 + 11;
                                             next_random = next_random*((long )25214903917 + 11);
                                            target = _table[(next_random >> 16)%(int)TableSize];
                                            if (target == 0) target = next_random%(_vocabSize - 1) + 1;
                                            if (target == word) continue;
                                            label = 0;
                                        }
                                        l2 = target*_layer1Size;
                                        f = 0;
                                        for (c = 0; c < _layer1Size; c++) f += neu1[c]*_syn1Neg[c + l2];
                                        if (f > MAX_EXP) g = (label - 1)*_alpha;
                                        else if (f < -MAX_EXP) g = (label - 0)*_alpha;
                                        else
                                            g = (label - _expTable[(int) ((f + MAX_EXP)*(EXP_TABLE_SIZE/MAX_EXP/2))])*
                                                _alpha;
                                        for (c = 0; c < _layer1Size; c++) neu1e[c] += g*_syn1Neg[c + l2];
                                        for (c = 0; c < _layer1Size; c++) _syn1Neg[c + l2] += g*neu1[c];
                                    }
                                // hidden -> in
                                for (a = b; a < _window*2 + 1 - b; a++)
                                    if (a != _window)
                                    {
                                        c = sentence_position - _window + a;
                                        if (c < 0) continue;
                                        if (c >= sentence_length) continue;
                                        last_word = sen[c];
                                        if (last_word == -1) continue;
                                        for (c = 0; c < _layer1Size; c++) _syn0[c + last_word*_layer1Size] += neu1e[c];
                                    }
                            }
                        }
                        else
                        {
                            //train skip-gram
                            for (a = b; a < _window*2 + 1 - b; a++)
                                if (a != _window)
                                {
                                    c = sentence_position - _window + a;
                                    if (c < 0) continue;
                                    if (c >= sentence_length) continue;
                                    last_word = sen[c];
                                    if (last_word == -1) continue;
                                    l1 = last_word*_layer1Size;
                                    for (c = 0; c < _layer1Size; c++) neu1e[c] = 0;
                                    // HIERARCHICAL SOFTMAX
                                    if (_hs !=0)
                                        for (d = 0; d < _vocab[word].CodeLen; d++)
                                        {
                                            f = 0;
                                            l2 = _vocab[word].Point[d]*_layer1Size;
                                            // Propagate hidden -> output
                                            for (c = 0; c < _layer1Size; c++) f += _syn0[c + l1]*_syn1[c + l2];
                                            if (f <= -MAX_EXP) continue;
                                            if (f >= MAX_EXP) continue;
                                            f = _expTable[(int) ((f + MAX_EXP)*(EXP_TABLE_SIZE/MAX_EXP/2))];
                                            // 'g' is the gradient multiplied by the learning rate
                                            g = (1 - _vocab[word].Code[d] - f)*_alpha;
                                            // Propagate errors output -> hidden
                                            for (c = 0; c < _layer1Size; c++) neu1e[c] += g*_syn1[c + l2];
                                            // Learn weights hidden -> output
                                            for (c = 0; c < _layer1Size; c++) _syn1[c + l2] += g*_syn0[c + l1];
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
//                                                next_random = next_random*(unsigned long long ) 25214903917 + 11;
//                                                target = table[(next_random >> 16)%table_size];
                                                next_random = next_random*((long )25214903917 + 11);
                                            target = _table[(next_random >> 16)%(int)TableSize];
                                                if (target == 0) target = next_random%(_vocabSize - 1) + 1;
                                                if (target == word) continue;
                                                label = 0;
                                            }
                                            l2 = target*_layer1Size;
                                            f = 0;
                                            for (c = 0; c < _layer1Size; c++) f += _syn0[c + l1]*_syn1Neg[c + l2];
                                            if (f > MAX_EXP) g = (label - 1)*_alpha;
                                            else if (f < -MAX_EXP) g = (label - 0)*_alpha;
                                            else
                                                g = (label - _expTable[(int) ((f + MAX_EXP)*(EXP_TABLE_SIZE/MAX_EXP/2))])*
                                                    _alpha;
                                            for (c = 0; c < _layer1Size; c++) neu1e[c] += g*_syn1Neg[c + l2];
                                            for (c = 0; c < _layer1Size; c++) _syn1Neg[c + l2] += g*_syn0[c + l1];
                                        }
                                    // Learn weights input -> hidden
                                    for (c = 0; c < _layer1Size; c++) _syn0[c + l1] += neu1e[c];
                                }
                        }
                        sentence_position++;
                        if (sentence_position >= sentence_length)
                        {
                            sentence_length = 0;
                        }
                    }
                    neu1 = null;
                    neu1e = null;

                    //fclose(fi);
                    //free(neu1);
                    //free(neu1e);
                    //pthread_exit(NULL);
                }
            }
            //fseek(fi, file_size / (long long)num_threads * (long long)id, SEEK_SET);
        }

        public void TrainModel()
        {
            long d;
            //FILE* fo;
            //pthread_t* pt = (pthread_t*)malloc(num_threads * sizeof(pthread_t));
            Thread[] pt = new Thread[_numThreads];
            //printf("Starting training using file %s\n", train_file);
            Console.WriteLine("Starting training using file {0}\n", _trainFile);
            _startingAlpha = _alpha;
            if (!string.IsNullOrEmpty(_readVocabFile)) 
                ReadVocab(); 
            else 
                LearnVocabFromTrainFile();
            if (!string.IsNullOrEmpty(_saveVocabFile)) SaveVocab();
            if (string.IsNullOrEmpty(_outputFile)) return;
            InitNet();
            if (_negative > 0) InitUnigramTable();
            _start = DateTime.Now;
            //for (a = 0; a < num_threads; a++) pthread_create(&pt[a], NULL, TrainModelThread, (void*)a);
            //for (a = 0; a < num_threads; a++) pthread_join(pt[a], NULL);
            for (int a = 0; a < _numThreads; a++)
            {
                pt[a] = new Thread(TrainModelThreadStart);
                Int32 idObject = a;
                pt[a].Start(idObject);
            }
            for (int a = 0; a < _numThreads; a++) pt[a].Join();
            using (FileStream stream = new FileStream(_outputFile,FileMode.OpenOrCreate))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream))
                {
                    using (TextWriter textWriter = new StreamWriter(stream))
                    {


                        //fo = fopen(output_file, "wb");
                        long b;
                        if (_classes == 0)
                        {
                            // Save the word vectors
                            //fprintf(fo, "%lld %lld\n", vocab_size, layer1_size);
                            if (_binary > 0)
                            {
                                //binaryWriter.Write(string.Format("{0} {1}\n", _vocabSize, _layer1Size));
                                binaryWriter.Write(_vocabSize);
                                binaryWriter.Write(' ');
                                binaryWriter.Write(_layer1Size);
                                binaryWriter.Write('\n');
                            }
                                
                            else
                                textWriter.Write("{0} {1}\n", _vocabSize, _layer1Size);

                            for (int a = 0; a < _vocabSize; a++)
                            {
                                //fprintf(fo, "%s ", vocab[a].word);
                                //textWriter.Write(String.Concat(_vocab[a].Word, " "));
                                
                                if (_binary > 0)
                                {
                                    //binaryWriter.Write(Encoding.UTF8.GetBytes(string.Concat(_vocab[a].Word, " ")));
                                    //binaryWriter.Write(_vocab[a].Word + " ");
                                    foreach (char t in _vocab[a].Word)
                                        binaryWriter.Write(t);
                                    binaryWriter.Write(' ');
                                    for (b = 0; b < _layer1Size; b++)
                                        binaryWriter.Write(BitConverter.GetBytes(_syn0[a*_layer1Size + b]));
                                    binaryWriter.Write('\n');
                                }

                                //fwrite(&syn0[a * layer1_size + b], sizeof(real), 1, fo);
                                else
                                {
                                    textWriter.Write(String.Concat(_vocab[a].Word, " "));
                                    for (b = 0; b < _layer1Size; b++)
                                        textWriter.Write(_syn0[a * _layer1Size + b]);
                                    textWriter.WriteLine();
                                }
                                    
                                //fprintf(fo, "%lf ", syn0[a * layer1_size + b]);
                               
                                //fprintf(fo, "\n");
                            }
                        }
                        else
                        {
                            // Run K-means on the word vectors
                            int clcn = (int) _classes, iter = 10, closeid;
                            int[] centcn = new int[_classes];
                            int[] cl = new int[_vocabSize];
                            double closev, x;
                            double[] cent = new double[_classes*_layer1Size];
                            for (int a = 0; a < _vocabSize; a++) cl[(int) a] = (int) a%clcn;
                            for (int a = 0; a < iter; a++)
                            {
                                for (b = 0; b < clcn*_layer1Size; b++) cent[b] = 0;
                                for (b = 0; b < clcn; b++) centcn[b] = 1;
                                long c;
                                for (c = 0; c < _vocabSize; c++)
                                {
                                    for (d = 0; d < _layer1Size; d++)
                                        cent[_layer1Size*cl[c] + d] += _syn0[c*_layer1Size + d];
                                    centcn[cl[c]]++;
                                }
                                for (b = 0; b < clcn; b++)
                                {
                                    closev = 0;
                                    for (c = 0; c < _layer1Size; c++)
                                    {
                                        cent[_layer1Size*b + c] /= centcn[b];
                                        closev += cent[_layer1Size*b + c]*cent[_layer1Size*b + c];
                                    }
                                    closev = Math.Sqrt(closev);
                                    for (c = 0; c < _layer1Size; c++) cent[_layer1Size*b + c] /= closev;
                                }
                                for (c = 0; c < _vocabSize; c++)
                                {
                                    closev = -10;
                                    closeid = 0;
                                    for (d = 0; d < clcn; d++)
                                    {
                                        x = 0;
                                        for (b = 0; b < _layer1Size; b++)
                                            x += cent[_layer1Size*d + b]*_syn0[c*_layer1Size + b];
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
                            for (int a = 0; a < _vocabSize; a++)
                            {
                                if (_binary > 0)
                                {
                                    binaryWriter.Write(string.Format("{0} {1}\n", _vocab[a].Word, cl[a]));
                                }
                                else
                                {
                                    textWriter.Write("{0} {1}\n", _vocab[a].Word, cl[a]);
                                }
                            }
                                
                            //printf(fo, "%s %d\n", vocab[a].word, cl[a]);
                            centcn = null;
                            cent = null;
                            cl = null;
                            //free(centcn);
                            //free(cent);
                            //free(cl);
                        }
                        //fclose(fo);
                    }
                }
            }
   
        }



    }



   

}



