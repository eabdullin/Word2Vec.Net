using System.Runtime.InteropServices;

namespace Word2Vec.Net
{
    public class Word2Vec
    {
        private readonly string _trainFile;
        private readonly string _outputFile;
        private readonly string _saveVocabFile;
        private readonly string _readVocabFile;
        private readonly int _binary;
        private readonly int _cbow;
        private readonly int _debugMode;
        private readonly int _window = 5;
        private readonly int _minCount;
        private readonly int _numThreads;
        private readonly int _layer1Size;
        private readonly long _iter;
        private readonly long _classes;
        private readonly double _alpha;
        private readonly double _sample;
        private readonly int _hs;
        private readonly int _negative;
        private readonly object _monitor = new object();
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

        [DllImport("wordvec/hello.dll")]
        private static extern void hello(string text);
        [DllImport("Word2VecLib2.dll")]
        private static extern void TrainModelWithParameters(string trainFileName,
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
            int window);

        public void TrainModel()
        {
            //hello("C# called");
            lock (_monitor)
            {
                TrainModelWithParameters(_trainFile, _outputFile, _saveVocabFile, _readVocabFile, _layer1Size, _debugMode, _binary, _cbow,_alpha, _sample, _hs, _negative, _numThreads, _iter, _minCount, _classes, _window);
            }

        }
    }
}



