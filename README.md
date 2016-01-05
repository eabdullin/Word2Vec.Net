# Word2Vec.Net
implementation Word2Vec(https://code.google.com/p/word2vec/) for .Net framework

#Getting Started

##For using 
```c#
            var builder = Word2VecBuilder.Create();

            if ((i = ArgPos("-train",  args)) > -1)
                builder.WithTrainFile(args[i + 1]);
            if ((i = ArgPos("-output", args)) > -1)
                builder.WithOutputFile(args[i + 1]);

            var word2Vec = builder.Build();
            word2Vec.TrainModel();
```
OR
```c#
            var word2Vec = Word2VecBuilder.Create()
                .WithTrainFile("trainingFile.txt")
                .WithOutputFile("outputFile.txt")
                .Build();
                
            word2Vec.TrainModel();
```

from Google word2vec:
Tools for computing distributed representtion of words
------------------------------------------------------

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
