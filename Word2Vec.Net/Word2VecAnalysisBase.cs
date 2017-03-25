using System;
using System.IO;
using Word2Vec.Net.Utils;

namespace Word2Vec.Net
{
  public class Word2VecAnalysisBase
  {
    public const long max_size = 2000; // max length of strings
    public const long max_w = 50; // max length of vocabulary entries
    public const long N = 40; // number of closest words that will be shown
    private readonly string file_name;

    /// <summary>
    ///   Basic class for analysis algorithms( distnace, analogy, commpute-accuracy)
    /// </summary>
    /// <param name="fileName"></param>
    public Word2VecAnalysisBase(string fileName)
    {
      file_name = fileName; //bestw = new string[N];


      InitVocub();
    }

    protected float[] M { get; set; }

    protected long Size { get; set; }

    protected char[] Vocab { get; set; }

    protected long Words { get; set; }

    public double MinimumDistance { get; set; } = 0.1;

    private void InitVocub()
    {
      using (var f = File.Open(file_name, FileMode.Open, FileAccess.Read))
      {
        //var text = ReadInt32(f);
        //int[] data = text.Split(' ').Select(int.Parse).ToArray();
        Words = f.ReadInt32();
        Size = f.ReadInt32();
        M = new float[Words * Size];
        Vocab = new char[Words * max_w];
        for (var b = 0; b < Words; b++)
        {
          var a = 0;
          var i = 0;
          var word = f.ReadWord();
          foreach (var ch in word)
          {
            Vocab[b * max_w + a] = ch;
            if (a < max_w && Vocab[b * max_w + a] != '\n')
              a++;
          }
          Vocab[b * max_w + a] = '\0';
          for (a = 0; a < Size; a++)
          {
            var bytes = new byte[4];
            f.Read(bytes, 0, 4);
            M[a + b * Size] = BitConverter.ToSingle(bytes, 0);
          }
          float len = 0;
          for (a = 0; a < Size; a++)
            len += M[a + b * Size] * M[a + b * Size];
          len = (float) Math.Sqrt(len);
          for (a = 0; a < Size; a++)
            M[a + b * Size] = M[a + b * Size] / len;
        }
      }
    }
  }
}