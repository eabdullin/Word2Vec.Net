namespace Word2Vec.Net
{
  public class Word2VecBuilder
  {
    private float _alpha = (float) 0.025;
    private int _binary;
    private int _cbow = 1;
    private long _classes;
    private int _debugMode = 2;

    private int _hs;
    private long _iter = 5;
    private int _layer1Size = 100;
    private int _minCount = 5;
    private int _negative = 5;
    private int _numThreads = 12;
    private string _outputFile;
    private string _readVocabFile;
    private float _sample = (float) 1e-3;
    private string _saveVocabFile;

    private string _trainFile;
    private int _window = 5;

    private Word2VecBuilder() { }

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

    public static Word2VecBuilder Create() { return new Word2VecBuilder(); }

    public Word2VecBuilder WithAlpha(float alpha)
    {
      _alpha = alpha;
      return this;
    }

    public Word2VecBuilder WithBinary(int binary)
    {
      _binary = binary;
      return this;
    }

    public Word2VecBuilder WithCBow(int cbow)
    {
      _cbow = cbow;
      return this;
    }

    public Word2VecBuilder WithClasses(int classes)
    {
      _classes = classes;
      return this;
    }

    public Word2VecBuilder WithDebug(int debugMode)
    {
      _debugMode = debugMode;
      return this;
    }

    public Word2VecBuilder WithHs(int hs)
    {
      _hs = hs;
      return this;
    }

    public Word2VecBuilder WithIter(int iter)
    {
      _iter = iter;
      return this;
    }

    public Word2VecBuilder WithMinCount(int count)
    {
      _minCount = count;
      return this;
    }

    public Word2VecBuilder WithNegative(int negative)
    {
      _negative = negative;
      return this;
    }

    public Word2VecBuilder WithOutputFile(string outputFileName)
    {
      _outputFile = outputFileName;
      return this;
    }

    public Word2VecBuilder WithReadVocubFile(string readVocubFileName)
    {
      _readVocabFile = readVocubFileName;
      return this;
    }

    public Word2VecBuilder WithSample(float sample)
    {
      _sample = sample;
      return this;
    }

    /// <summary>
    ///   adding file path for saving vocabulary
    /// </summary>
    /// <param name="saveVocubFileName">file path for saving vocabulary</param>
    /// <returns></returns>
    public Word2VecBuilder WithSaveVocubFile(string saveVocubFileName)
    {
      _saveVocabFile = saveVocubFileName;
      return this;
    }

    public Word2VecBuilder WithSize(int size)
    {
      _layer1Size = size;
      return this;
    }

    public Word2VecBuilder WithThreads(int threads)
    {
      _numThreads = threads;
      return this;
    }

    /// <summary>
    ///   adding train file path
    /// </summary>
    /// <param name="trainFileName">adding train file path</param>
    /// <returns></returns>
    public Word2VecBuilder WithTrainFile(string trainFileName)
    {
      _trainFile = trainFileName;
      return this;
    }

    public Word2VecBuilder WithWindow(int window)
    {
      _window = window;
      return this;
    }
  }
}