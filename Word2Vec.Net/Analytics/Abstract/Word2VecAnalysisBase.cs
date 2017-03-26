using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Word2Vec.Net.Analytics.Abstract
{
  public abstract class Word2VecAnalysisBase
  {
    public const long max_size = 2000; // max length of strings
    public const long max_w = 50; // max length of vocabulary entries
    public const long N = 40; // number of closest words that will be shown
    private readonly string file_name;

    protected Word2VecAnalysisBase(string fileName)
    {
      file_name = fileName; //bestw = new string[N];
      InitVocub();
    }

    protected float[] M { get; set; }

    protected long Size { get; set; }

    protected char[] Vocab { get; set; }

    protected long Words { get; set; }

    public double MinimumDistance { get; set; } = 0.999;

    private void InitVocub()
    {
      using (var fs = new FileStream(file_name, FileMode.Open, FileAccess.Read))
      {
        using (var reader = new StreamReader(fs, Encoding.UTF8))
        {
          Words = int.Parse(reader.ReadLine());
          Size = int.Parse(reader.ReadLine());
          M = new float[Words * Size];
          Vocab = new char[Words * max_w];
          for (var b = 0; b < Words; b++)
          {
            var a = 0;
            var i = 0;

            var line = reader.ReadLine()?.Split(new[]{ "\t" }, StringSplitOptions.RemoveEmptyEntries);
            if (line == null)
              continue;

            var word = line[0];
            foreach (var ch in word)
            {
              Vocab[b * max_w + a] = ch;
              if (a < max_w && Vocab[b * max_w + a] != '\n')
                a++;
            }
            Vocab[b * max_w + a] = '\0';
            var floats = GetFloats(line[1]);
            if (floats == null)
              continue;
            for (a = 0; a < floats.Length; a++)
            {
              M[a + b * Size] = floats[a];
            }
            float len = 0;
            for (a = 0; a < Size; a++)
              len += M[a + b * Size] * M[a + b * Size];
            len = (float) Math.Sqrt(len);
            for (a = 0; a < Size; a++)
              M[a + b * Size] = M[a + b * Size] / len;

            if (reader.EndOfStream)
              break;
          }
        }
      }
    }

    private float[] GetFloats(string s)
    {
      var bytes = Convert.FromBase64String(s);
      if (bytes.Length == 0)
        return null;

      var res = new List<float>();
      var buffer = new byte[sizeof(float)];
      for (int i = 0; i + 1 < bytes.Length; i+= sizeof(float))
      {
        Buffer.BlockCopy(bytes, i, buffer, 0, sizeof(float));
        res.Add(BitConverter.ToSingle(buffer, 0));
      }
      return res.ToArray();
    }
  }
}