using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Word2Vec.Net
{
    public class Word2VecBuilder
    {

        private string _trainFile;
        private string _outputFile;
        private string _saveVocabFile;
        private string _readVocabFile;
        private int _binary = 0;
        private int _cbow = 1;
        private int _debugMode = 2;
        private int _minCount = 5;
        private int _numThreads = 12;
        private int _layer1Size = 100;
        private long _iter = 5;
        private long _classes = 0;
        private float _alpha = (float) 0.025;
        private float _sample = (float) 1e-3;

        private int _hs = 0;
        private int _negative = 5;
        private int _window = 5;

        private Word2VecBuilder()
        {
        }

        public static Word2VecBuilder Create()
        {
            return new Word2VecBuilder();
        }

        public Word2VecBuilder WithSize(int size)
        {
            this._layer1Size = size;
            return this;
        }

        /// <summary>
        /// adding train file path
        /// </summary>
        /// <param name="trainFileName">adding train file path</param>
        /// <returns></returns>
        public Word2VecBuilder WithTrainFile(string trainFileName)
        {
            this._trainFile = trainFileName;
            return this;
        }

        /// <summary>
        /// adding file path for saving vocabulary
        /// </summary>
        /// <param name="saveVocubFileName">file path for saving vocabulary</param>
        /// <returns></returns>
        public Word2VecBuilder WithSaveVocubFile(string saveVocubFileName)
        {
            this._saveVocabFile = saveVocubFileName;
            return this;
        }
        public Word2VecBuilder WithReadVocubFile(string readVocubFileName)
        {
            this._readVocabFile = readVocubFileName;
            return this;
        }
        public Word2VecBuilder WithDebug(int debugMode)
        {
            this._debugMode = debugMode;
            return this;
        }
        public Word2VecBuilder WithBinary(int binary)
        {
            this._binary = binary;
            return this;
        }
        public Word2VecBuilder WithCBow(int cbow)
        {
            this._cbow = cbow;
            return this;
        }
        public Word2VecBuilder WithAlpha(float alpha)
        {
            this._alpha = alpha;
            return this;
        }
        public Word2VecBuilder WithOutputFile(string outputFileName)
        {
            this._outputFile = outputFileName;
            return this;
        }
        public Word2VecBuilder WithSample(float sample)
        {
            this._sample = sample;
            return this;
        }
        public Word2VecBuilder WithHs(int hs)
        {
            this._hs = hs;
            return this;
        }
        public Word2VecBuilder WithNegative(int negative)
        {
            this._negative = negative;
            return this;
        }
        public Word2VecBuilder WithThreads(int threads)
        {
            this._numThreads = threads;
            return this;
        }
        public Word2VecBuilder WithIter(int iter)
        {
            this._iter = iter;
            return this;
        }
        public Word2VecBuilder WithMinCount(int count)
        {
            this._minCount = count;
            return this;
        }
        public Word2VecBuilder WithClasses(int classes)
        {
            this._classes = classes;
            return this;
        }
        public Word2VecBuilder WithWindow(int window)
        {
            this._window = window;
            return this;
        }

        public Word2Vec Build()
        {
            return new Word2Vec(
                _trainFile,
                _outputFile,
                _saveVocabFile,
                _readVocabFile,
                _layer1Size,
                _debugMode,
                _binary,
                _cbow,
                _alpha,
                _sample,
                _hs,
                _negative,
                _numThreads,
                _iter,
                _minCount,
                _classes,
                _window
                );
        }
    }
}
