using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Word2Vec.Net;

namespace Word2Vec.UnitTest
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestAppInit()
    {
      var word2Vec = Word2VecBuilder.Create()
                                    .WithTrainFile("TestData/faust1.txt") // Use text data to train the model;
                                    .WithOutputFile("faust1.bin")
                                    //Use to save the resulting word vectors / word clusters
                                    .WithSize(200) //Set size of word vectors; default is 100
                                    .WithSaveVocubFile("faust1.dic") //The vocabulary will be saved to <file>
                                    .WithBinary(1) //Save the resulting vectors in binary moded; default is 0 (off)
                                    .WithCBow(1)
                                    //Use the continuous bag of words model; default is 1 (use 0 for skip-gram model)
                                    .WithAlpha(0.05f)
                                    //Set the starting learning rate; default is 0.025 for skip-gram and 0.05 for CBOW
                                    .WithWindow(7) //Set max skip length between words; default is 5
                                    .WithSample((float)1e-3)
                                    //Set threshold for occurrence of words. Those that appear with higher frequency in the training data twill be randomly down-sampled; default is 1e-3, useful range is (0, 1e-5)
                                    .WithHs(0) //Use Hierarchical Softmax; default is 0 (not used)
                                    .WithNegative(5)
                                    //Number of negative examples; default is 5, common values are 3 - 10 (0 = not used)
                                    .WithThreads(5) //Use <int> threads (default 12)
                                    .WithIter(5) //Run more training iterations (default 5)
                                    .WithMinCount(5)
                                    //This will discard words that appear less than <int> times; default is 5
                                    .WithClasses(0)
                                    //Output word classes rather than word vectors; default number of classes is 0 (vectors are written)
                                    .Build();

      word2Vec.TrainModel();
    }

    [TestMethod]
    public void TestDistance()
    {
      var distance = new Distance("faust1.bin"){ MinimumDistance = 0.99};
      var bestwords = distance.Search("Gott");
      Assert.IsNotNull(bestwords);
      Assert.IsTrue(bestwords.Count == 743);

      bestwords = distance.Search("ich");
      Assert.IsNotNull(bestwords);
      Assert.IsTrue(bestwords.Count == 743);
    }

    [TestMethod]
    public void TestWordAnalogy()
    {
      var analogy = new WordAnalogy("faust1.bin") { MinimumDistance = 0 };
      var result = analogy.Search(new[]{"Gott", "Natur"});
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Count == 745);
    }
  }
}