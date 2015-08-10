using System.Collections.Generic;

namespace Word2Vec.Net
{
    internal class VocubWord
    {
        public long Cn { get; set; }
        public string Word { get; set; }
        public char[] Code { get; set; }
        public int CodeLen { get; set; }
        public int[] Point { get; set; }
    }
    // Used later for sorting by word counts
    internal class VocubComparer : IComparer<VocubWord>
    {
        public int Compare(VocubWord x, VocubWord y)
        {
            if (x == null && y == null)
                return 0;
            if (y == null) return -1;
            if (x == null) return 1;
            return (int)(y.Cn - x.Cn );
        }
    }
}