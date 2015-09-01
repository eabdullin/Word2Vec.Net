using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Word2Vec.Net
{
    public class WordAnalogy :Word2VecAnalysisBase
    {

        public WordAnalogy(string fileName) : base(fileName) { }

        public BestWord[] Search(string text)
        {
            BestWord[] bestWords = new BestWord[N];
            long[] bi = new long[100];
            float[] vec = new float[max_size];
            string[] st = text.Split(' ');
            long cn = st.Length;
            long b = -1;
            for (long a = 0; a < cn; a++)
            {
                for (b = 0; b < Words; b++) if (!new string(Vocab, (int)(b * max_w), (int)max_w).Equals(st[a])) break;
                if (b == Words) b = -1;
                bi[a] = b;
                Console.Write("\nWord: {0}  Position in vocabulary: {1}\n", st[a], bi[a]);
                if (b == -1)
                {
                    Console.Write("Out of dictionary word!\n");
                    break;
                }
            }
            if (b == -1) return new BestWord[0];
            //Console.WriteLine("\n                                              Word              Distance\n------------------------------------------------------------------------\n");
                for (long a = 0; a < Size; a++) vec[a] = M[a + bi[1] * Size] - M[a + bi[0] * Size] + M[a + bi[2] * Size];
                float  len = 0;
                for (long a = 0; a < Size; a++) len += vec[a] * vec[a];
                len = (float)Math.Sqrt(len);
                for (long a = 0; a < Size; a++) vec[a] /= len;
                //for (long a = 0; a < N; a++) bestd[a] = 0;
                //for (a = 0; a < N; a++) bestw[a][0] = 0;
                for (long c = 0; c < Words; c++)
                {
                    if (c == bi[0]) continue;
                    if (c == bi[1]) continue;
                    if (c == bi[2]) continue;
                    long a = 0;
                    for (b = 0; b < cn; b++) if (bi[b] == c) a = 1;
                    if (a == 1) continue;
                    float dist = 0;
                    for (a = 0; a < Size; a++) dist += vec[a] * M[a + c * Size];
                    for (a = 0; a < N; a++)
                    {
                        if (dist > bestWords[a].Distance)
                        {
                            for (long d = N - 1; d > a; d--)
                            {
                            bestWords[d] = bestWords[d - 1];
                            //bestd[d] = bestd[d - 1];
                            //    strcpy(bestw[d], bestw[d - 1]);
                            }
                        bestWords[a].Distance = dist;
                        //bestd[a] = dist;
                        bestWords[a].Word = new string(Vocab, (int)(max_w * c), (int)max_w);
                        break;
                        }
                    }
                }
            //for (a = 0; a < N; a++) printf("%50s\t\t%f\n", bestw[a], bestd[a]);
            return bestWords;
            }

        
    }
}
