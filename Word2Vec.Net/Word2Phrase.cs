using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Word2Vec.Net
{
    public class Word2Phrase
    {
        #region Constants
        private const int MaxCodeLength = 40;
        private const int MAX_STRING = 60;
        private const int VocabHashSize = 5000000;
        #endregion

        #region Word2phrase start params
        private readonly string _trainFile;
        private readonly string _outputFile;
        private readonly int _debugMode;
        private readonly int _minCount;
      
        #endregion
        private VocubWord[] _vocab;
        private int _minReduce = 1;
        private readonly int[] _vocabHash;
        private int _vocabMaxSize = 1000;
        private int _vocabSize;
        private long _trainWords;
        private int _threshold = 100;

       
        public Word2Phrase(
            string trainFileName,
            string outPutfileName,
            int debugMode,
            int minCount,
            int threshold
           
            )
        {
            _trainFile = trainFileName;
            _outputFile = outPutfileName;
            _vocab = new VocubWord[_vocabMaxSize];
            _vocabHash = new int[VocabHashSize]; 
            _debugMode = debugMode;
            _minCount = minCount;
            _threshold = threshold;
        }



        private uint GetWordHash(string word)
        {
            int a;
            ulong hash = 0;
            for (a = 0; a < word.Length; a++) hash = hash * 257 + word[a];
            hash = hash % VocabHashSize;
            return (uint)hash;
        }

        private int SearchVocab(string word)
        {
            var hash = GetWordHash(word);
            while (true)
            {
                if (_vocabHash[hash] == -1) return -1;
                if (word.Equals(_vocab[_vocabHash[hash]].Word))
                    return _vocabHash[hash];
                hash = (hash + 1) % VocabHashSize;
            }
            //return -1;
        }

        private int AddWordToVocab(string word)
        {

            _vocab[_vocabSize].Word = word;
            _vocab[_vocabSize].Cn = 0;
            _vocabSize++;
            // Resize array if needed
            if (_vocabSize + 2 >= _vocabMaxSize)
            {
                _vocabMaxSize += 10000;
                Array.Resize(ref _vocab, _vocabMaxSize);
            }
            uint hash = GetWordHash(word);
            while (_vocabHash[hash] != -1) hash = (hash + 1) % VocabHashSize;
            _vocabHash[hash] = _vocabSize - 1;
            return _vocabSize - 1;
        }

        private void SortVocab()
        {
            // Sort the vocabulary and keep </s> at the first position
            Array.Sort(_vocab, 1, _vocabSize - 1, new VocubComparer());
            for (var a = 0; a < VocabHashSize; a++) _vocabHash[a] = -1;
            int size = _vocabSize;
            _trainWords = 0;
            for (var a = 0; a < size; a++)
            {
                // Words occuring less than min_count times will be discarded from the vocab
                if (_vocab[a].Cn < _minCount && (a != 0))
                {
                    _vocabSize--;
                    _vocab[a].Word = null;
                }
                else
                {
                    // Hash will be re-computed, as after the sorting it is not actual
                    var hash = GetWordHash(_vocab[a].Word);
                    while (_vocabHash[hash] != -1) hash = (hash + 1) % VocabHashSize;
                    _vocabHash[hash] = a;
                    _trainWords += _vocab[a].Cn;
                }
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

        private void ReduceVocab()
        {
            var b = 0;
            for (var a = 0; a < _vocabSize; a++)
            {
                if (_vocab[a].Cn > _minReduce)
                {
                    _vocab[b].Cn = _vocab[a].Cn;
                    _vocab[b].Word = _vocab[a].Word;
                    b++;
                }
                else _vocab[a].Word = null;
            }
            _vocabSize = b;
            for (var a = 0; a < VocabHashSize; a++) _vocabHash[a] = -1;

            for (var a = 0; a < _vocabSize; a++)
            {
                // Hash will be re-computed, as it is not actual
                uint hash = GetWordHash(_vocab[a].Word);
                while (_vocabHash[hash] != -1) hash = (hash + 1) % VocabHashSize;
                _vocabHash[hash] = a;
            }
            _minReduce++;
            GC.Collect();
        }

        private void LearnVocabFromTrainFile()
        {
            int i;
            long start = 1;
            for (var a = 0; a < VocabHashSize; a++) _vocabHash[a] = -1;
            string last_word = "";
            using (var fin = File.OpenText(_trainFile))
            {
                if (fin == StreamReader.Null)
                {
                    throw new InvalidOperationException("ERROR: training data file not found!\n");
                }
                _vocabSize = 0;

                string line;
                Regex regex = new Regex("\\s");
                AddWordToVocab("</s>");
                while ((line = fin.ReadLine()) != null)
                {
                    
                    string[] words = regex.Split(line);

                    foreach (var word in words)
                    {
                        if (string.IsNullOrWhiteSpace(word))
                        {
                            start = 1;
                            continue;
                        }
                        else
                        {
                            start = 0;
                        }
                        _trainWords++;
                        if ((_debugMode > 1) && (_trainWords % 100000 == 0))
                        {
                            Console.Write("{0}K \r", _trainWords / 1000);
                            //printf("%lldK%c", train_words / 1000, 13);
                            //fflush(stdout);
                        }
                        i = SearchVocab(word);
                        if (i == -1)
                        {
                            var a = AddWordToVocab(word);
                            _vocab[a].Cn = 1;
                        }
                        else
                            _vocab[i].Cn++;


                      
                        if (start!=0) continue;
                        string bigram_word = last_word + "_" + word;
                        last_word = word;
                       
                        i = SearchVocab(bigram_word);
                        if (i == -1)
                        {
                           var a = AddWordToVocab(bigram_word);
                            _vocab[a].Cn = 1;
                        }
                        else _vocab[i].Cn++;




                        if (_vocabSize > VocabHashSize * 0.7)
                            ReduceVocab();
                    }
                    if (fin.EndOfStream) break;
                }
                SortVocab();
                if (_debugMode > 0)
                {
                    Console.WriteLine("Vocab size: {0}", _vocabSize);
                    Console.WriteLine("Words in train file: {0}", _trainWords);
                }
              
            }


        }


        public void TrainModel()
        {
            Regex splitRegex = new Regex("\\s");
            long d,cn=0,oov,i,pb=0,li=-1,pa=0,pab=0;
            float score = 0;
            string word="";
            string lastWord = "";
            Console.WriteLine("Starting training using file {0}\n", _trainFile);
            LearnVocabFromTrainFile();
           
            using (StreamReader fi = File.OpenText(_trainFile))
            {
                using (var stream = new StreamWriter(_outputFile, false, Encoding.UTF8))
                {
                    string line;
                    bool loopEnd = false;
                    while (!loopEnd && (line = fi.ReadLine()) != null)
                    {
                        string[] words = splitRegex.Split(line);
                        foreach (var s in words)
                        {
                            word = s;


                            if (string.IsNullOrWhiteSpace(word))
                            {
                                stream.Write("\n");
                                continue;
                            }
                            
                            cn++;
                            if ((_debugMode > 1) && (cn % 100000 == 0))
                            {
                                Console.Write("{0}K \r", cn / 1000);
                            }
                            oov = 0;
                            i = SearchVocab(word);
                            if (i == -1)
                            {
                                oov = 1;
                            }
                            else
                            {
                                pb = _vocab[i].Cn;
                            }
                            if (li == -1) oov = 1;
                            li = i;
                            string bigram_word = lastWord+"_"+word;
                            i = SearchVocab(bigram_word);
                            if (i == -1)
                            {
                                oov = 1;
                            }
                            else
                            {
                                pab = _vocab[i].Cn;
                            }
                            if (pa < _minCount) oov = 1;
                            if (pb < _minCount) oov = 1;

                            if (oov!=0) score = 0;
                            else
                                score = (pab - _minCount) / (float)pa / (float)pb * (float)_trainWords;
                            if (score > _threshold)
                            {
                                stream.Write("_" + word);
                                pb = 0;
                            }
                            else stream.Write(" " + word);
                            pa = pb;

                            lastWord = word;
                            
                        }
                        if (fi.EndOfStream)
                        {
                            loopEnd = true;
                            break;
                        }

                    }

                }
            }

            GC.Collect();
           
        }

    }

}