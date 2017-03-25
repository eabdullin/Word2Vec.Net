using System;
using System.Collections.Generic;

namespace Word2Vec.Net
{
  public class Distance : Word2VecAnalysisBase
  {
    /// <summary>
    /// </summary>
    /// <param name="fileName">path to binary file created by Word2Vec</param>
    public Distance(string fileName) : base(fileName) { }

    /// <summary>
    ///   search nearest words to
    ///   <param name="intext"></param>
    /// </summary>
    /// <returns>nearest words</returns>
    public Dictionary<string, double> Search(string intext)
    {
      var bestWords = new Dictionary<string, double>();
      var bi = new long[100];
      var vec = new float[max_size];
      var st = intext.Split(' ');
      var cn = st.Length;
      long b = -1;
      for (long a = 0; a < cn; a++)
      {
        for (b = 0; b < Words; b++)
        {
          var word = new string(Vocab, (int) (b * max_w), (int) max_w).Replace("\0", string.Empty);
          if (word.Equals(st[a]))
            break;
        }
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
        vec[a] = 0;
      for (b = 0; b < cn; b++)
      {
        if (bi[b] == -1)
          continue;
        for (long a = 0; a < Size; a++)
          vec[a] += M[a + bi[b] * Size];
      }
      float len = 0;
      for (long a = 0; a < Size; a++)
        len += vec[a] * vec[a];
      len = (float) Math.Sqrt(len);
      for (long a = 0; a < Size; a++)
        vec[a] /= len;
      for (long c = 0; c < Words; c++)
      {
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
      return bestWords;
    }
  }
}