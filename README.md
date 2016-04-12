# Word2Vec.Net
[![Gitter](https://badges.gitter.im/eabdullin/Word2Vec.Net.svg)](https://gitter.im/eabdullin/Word2Vec.Net?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

implementation Word2Vec(https://code.google.com/p/word2vec/) for .Net framework

#Getting Started

##Using
```c#
            var builder = Word2VecBuilder.Create();

            if ((i = ArgPos("-train",  args)) > -1)
                builder.WithTrainFile(args[i + 1]);
            if ((i = ArgPos("-output", args)) > -1)
                builder.WithOutputFile(args[i + 1]);
            //to all other parameters will be set default values
            var word2Vec = builder.Build();
            word2Vec.TrainModel();
            var distance = new Distance(args[i + 1]);
            BestWord[] bestwords = distance.Search("some_word");
```
OR
```c#
//more explicit option
		string trainfile="C:/data.txt";
		string outputFileName = "C:/output.bin";
		var word2Vec = Word2VecBuilder.Create()
			.WithTrainFile(trainfile)// Use text data to train the model;
			.WithOutputFile(outputFileName)//Use to save the resulting word vectors / word clusters
			.WithSize(200)//Set size of word vectors; default is 100
			.WithSaveVocubFile()//The vocabulary will be saved to <file>
			.WithDebug(2)//Set the debug mode (default = 2 = more info during training)
			.WithBinary(1)//Save the resulting vectors in binary moded; default is 0 (off)
			.WithCBow(1)//Use the continuous bag of words model; default is 1 (use 0 for skip-gram model)
			.WithAlpha(0.05)//Set the starting learning rate; default is 0.025 for skip-gram and 0.05 for CBOW
			.WithWindow(7)//Set max skip length between words; default is 5
			.WithSample((float) 1e-3)//Set threshold for occurrence of words. Those that appear with higher frequency in the training data twill be randomly down-sampled; default is 1e-3, useful range is (0, 1e-5)
			.WithHs(0)//Use Hierarchical Softmax; default is 0 (not used)
			.WithNegative(5)//Number of negative examples; default is 5, common values are 3 - 10 (0 = not used)
			.WithThreads(5)//Use <int> threads (default 12)
			.WithIter(5)//Run more training iterations (default 5)
			.WithMinCount(5)//This will discard words that appear less than <int> times; default is 5
			.WithClasses(0)//Output word classes rather than word vectors; default number of classes is 0 (vectors are written)
			.Build();
			
            word2Vec.TrainModel();
			
		var distance = new Distance(outputFile);
		BestWord[] bestwords = distance.Search("some_word");
```

##Information from Google word2vec:
###Tools for computing distributed representtion of words

We provide an implementation of the Continuous Bag-of-Words (CBOW) and the Skip-gram model (SG), as well as several demo scripts.

Given a text corpus, the word2vec tool learns a vector for every word in the vocabulary using the Continuous
Bag-of-Words or the Skip-Gram neural network architectures. The user should to specify the following:
 - desired vector dimensionality
 - the size of the context window for either the Skip-Gram or the Continuous Bag-of-Words model
 - training algorithm: hierarchical softmax and / or negative sampling
 - threshold for downsampling the frequent words 
 - number of threads to use
 - the format of the output word vector file (text or binary)

Usually, the other hyper-parameters such as the learning rate do not need to be tuned for different training sets. 

The script demo-word.sh downloads a small (100MB) text corpus from the web, and trains a small word vector model. After the training
is finished, the user can interactively explore the similarity of the words.

More information about the scripts is provided at https://code.google.com/p/word2vec/
