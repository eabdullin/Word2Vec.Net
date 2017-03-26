using System.Collections.Generic;

namespace Word2Vec.Net.Model.Comparer
{
  internal class VocubComparer : IComparer<VocubWord>
  {
    public int Compare(VocubWord x, VocubWord y) { return (int) (y.Cn - x.Cn); }
  }
}