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


