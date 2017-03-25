using System;
using System.Collections.Generic;

namespace Word2Vec.Net
{
  public class WordAnalogy : Word2VecAnalysisBase
  {
    public WordAnalogy(string fileName) : base(fileName) { }

    public Dictionary<string, double> Search(string text)
    {
      var bestWords = new Dictionary<string, double>();
      var bi = new long[100];
      var vec = new float[max_size];
      var st = text.Split(' ');
      long cn = st.Length;
      long b = -1;
      for (long a = 0; a < cn; a++)
      {
        for (b = 0; b < Words; b++)
          if (!new string(Vocab, (int)(b * max_w), (int)max_w).Equals(st[a]))
            break;
        if (b == Words)
          b = -1;
        bi[a] = b;
        if (b == -1)
        {
          break;
        }
      }
      if (b == -1)
        return null;
      for (long a = 0; a < Size; a++)
        vec[a] = M[a + bi[1] * Size] - M[a + bi[0] * Size] + M[a + bi[2] * Size];
      float len = 0;
      for (long a = 0; a < Size; a++)
        len += vec[a] * vec[a];
      len = (float)Math.Sqrt(len);
      for (long a = 0; a < Size; a++)
        vec[a] /= len;
      for (long c = 0; c < Words; c++)
      {
        if (c == bi[0])
          continue;
        if (c == bi[1])
          continue;
        if (c == bi[2])
          continue;
        long a = 0;
        for (b = 0; b < cn; b++)
          if (bi[b] == c)
            a = 1;
        if (a == 1)
          continue;
        float dist = 0;
        for (a = 0; a < Size; a++)
          dist += vec[a] * M[a + c * Size];
        for (a = 0; a < N; a++)
          if (dist > MinimumDistance)
          {
            bestWords.Add(new string(Vocab, (int)(max_w * c), (int)max_w).Replace("\0", string.Empty).Trim(), dist);
            break;
          }
      }
      //for (a = 0; a < N; a++) printf("%50s\t\t%f\n", bestw[a], bestd[a]);
      return bestWords;
    }
  }
}