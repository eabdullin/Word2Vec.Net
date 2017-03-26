using System;

namespace Word2Vec.Net.Model
{
  internal struct VocubWord : IComparable<VocubWord>
  {
    public long Cn { get; set; }
    public string Word { get; set; }
    public char[] Code { get; set; }
    public int CodeLen { get; set; }
    public int[] Point { get; set; }
    public int CompareTo(VocubWord other) { return (int) (Cn - other.Cn); }
  }

  // Used later for sorting by word counts
}