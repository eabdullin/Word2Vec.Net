using System;
using System.Linq;
using Word2Vec.Net;

namespace Work2VecConsoleApp
{
  internal class Program
  {
    private static int ArgPos(string str, string[] args)
    {
      for (var a = 0; a < args.Length; a++)
        if (str.Equals(args[a]))
        {
          if (a == args.Length - 1)
            throw new ArgumentException(string.Format("Argument missing for {0}", str));
          return a;
        }
      return -1;
    }

    private static void Main(string[] args)
    {
      if (args.Length == 0)
      {
        Console.WriteLine("WORD VECTOR estimation toolkit v 0.1c\n");
        Console.WriteLine("Options:");
        Console.WriteLine("Parameters for training:");
        Console.WriteLine("\t-train <file>");
        Console.WriteLine("\t\tUse text data from <file> to train the model");
        Console.WriteLine("\t-output <file>");
        Console.WriteLine("\t\tUse <file> to save the resulting word vectors / word clusters");
        Console.WriteLine("\t-size <int>");
        Console.WriteLine("\t\tSet size of word vectors; default is 100");
        Console.WriteLine("\t-window <int>");
        Console.WriteLine("\t\tSet max skip length between words; default is 5");
        Console.WriteLine("\t-sample <float>");
        Console.WriteLine(
          "\t\tSet threshold for occurrence of words. Those that appear with higher frequency in the training data");
        Console.WriteLine("\t\twill be randomly down-sampled; default is 1e-3, useful range is (0, 1e-5)");
        Console.WriteLine("\t-hs <int>");
        Console.WriteLine("\t\tUse Hierarchical Softmax; default is 0 (not used)");
        Console.WriteLine("\t-negative <int>");
        Console.WriteLine(
          "\t\tNumber of negative examples; default is 5, common values are 3 - 10 (0 = not used)");
        Console.WriteLine("\t-threads <int>");
        Console.WriteLine("\t\tUse <int> threads (default 12)");
        Console.WriteLine("\t-iter <int>");
        Console.WriteLine("\t\tRun more training iterations (default 5)");
        Console.WriteLine("\t-min-count <int>");
        Console.WriteLine("\t\tThis will discard words that appear less than <int> times; default is 5");
        Console.WriteLine("\t-alpha <float>");
        Console.WriteLine("\t\tSet the starting learning rate; default is 0.025 for skip-gram and 0.05 for CBOW");
        Console.WriteLine("\t-classes <int>");
        Console.WriteLine(
          "\t\tOutput word classes rather than word vectors; default number of classes is 0 (vectors are written)");
        Console.WriteLine("\t-debug <int>");
        Console.WriteLine("\t\tSet the debug mode (default = 2 = more info during training)");
        Console.WriteLine("\t-binary <int>");
        Console.WriteLine("\t\tSave the resulting vectors in binary moded; default is 0 (off)");
        Console.WriteLine("\t-save-vocab <file>");
        Console.WriteLine("\t\tThe vocabulary will be saved to <file>");
        Console.WriteLine("\t-read-vocab <file>");
        Console.WriteLine("\t\tThe vocabulary will be read from <file>, not constructed from the training data");
        Console.WriteLine("\t-cbow <int>");
        Console.WriteLine("\t\tUse the continuous bag of words model; default is 1 (use 0 for skip-gram model)");
        Console.WriteLine("Examples:");
        Console.WriteLine(
          "./word2vec -train data.txt -output vec.txt -size 200 -window 5 -sample 1e-4 -negative 5 -hs 0 -binary 0 -cbow 1 -iter 3");

        return;
      }
      int i;
      var builder = Word2VecBuilder.Create();
      var outputFileName = string.Empty;
      if ((i = ArgPos("-train", args)) > -1)
        builder.WithTrainFile(args[i + 1]);
      if ((i = ArgPos("-output", args)) > -1)
      {
        outputFileName = args[i + 1];
        builder.WithOutputFile(outputFileName);
      }

      if ((i = ArgPos("-size", args)) > -1)
        builder.WithSize(int.Parse(args[i + 1]));
      if ((i = ArgPos("-save-vocab", args)) > -1)
        builder.WithSaveVocubFile(args[i + 1]);
      if ((i = ArgPos("-read-vocab", args)) > -1)
        builder.WithReadVocubFile(args[i + 1]);
      if ((i = ArgPos("-debug", args)) > -1)
        builder.WithDebug(int.Parse(args[i + 1]));
      if ((i = ArgPos("-binary", args)) > -1)
        builder.WithBinary(int.Parse(args[i + 1]));
      if ((i = ArgPos("-cbow", args)) > -1)
        builder.WithCBow(int.Parse(args[i + 1]));
      if ((i = ArgPos("-alpha", args)) > -1)
        builder.WithAlpha(float.Parse(args[i + 1]));

      if ((i = ArgPos("-window", args)) > -1)
        builder.WithWindow(int.Parse(args[i + 1]));
      if ((i = ArgPos("-sample", args)) > -1)
        builder.WithSample(float.Parse(args[i + 1]));
      if ((i = ArgPos("-hs", args)) > -1)
        builder.WithHs(int.Parse(args[i + 1]));
      if ((i = ArgPos("-negative", args)) > -1)
        builder.WithNegative(int.Parse(args[i + 1]));
      if ((i = ArgPos("-threads", args)) > -1)
        builder.WithThreads(int.Parse(args[i + 1]));
      if ((i = ArgPos("-iter", args)) > -1)
        builder.WithIter(int.Parse(args[i + 1]));
      if ((i = ArgPos("-min-count", args)) > -1)
        builder.WithMinCount(int.Parse(args[i + 1]));
      if ((i = ArgPos("-classes", args)) > -1)
        builder.WithClasses(int.Parse(args[i + 1]));
      var word2Vec = builder.Build();
      word2Vec.TrainModel();

      var distance = new Distance(outputFileName);
      while (true)
      {
        Console.WriteLine("Distance: Enter word or sentence (EXIT to break): ");
        var text = Console.ReadLine();
        if (text == null || text.ToLower().Equals("exit"))
          break;
        var result = distance.Search(text);
        Console.WriteLine(
          "\n                                              Word       Cosine distance\n------------------------------------------------------------------------");
        foreach (var bestWord in result.Where(x => !string.IsNullOrEmpty(x.Word)))
          Console.WriteLine("{0}\t\t{1}", bestWord.Word, bestWord.Distance);
        Console.WriteLine();
      }
    }
  }
}