// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Distance.cs" >
//   Create on 19:43:33 by Еламан Абдуллин
// </copyright>
// <summary>
//   Defines the Distance type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;

namespace Word2Vec.Net
{
    public class Distance 
    {
        const long max_size = 2000;         // max length of strings
        const long N = 40;                  // number of closest words that will be shown
        const long max_w = 50;              // max length of vocabulary entries
          //string[] bestw;
        private string file_name;
        string[] st;
          float dist;
        float len;
        //float[] bestd;
        float[] vec;
        long words;
        long size;
        long a;
        long b;
        long c;
        long d;
        long cn;
        long[] bi;
        char ch;
          float[] M;
          char[] vocab;
        private BestWord[] bestWords;


        public Distance(string fileName)
        {
            file_name = fileName;
            //bestw = new string[N];
            bestWords = new BestWord[N];
           
            InitVocub();
        }

        public BestWord[] Search(string intext)
        {

            st = intext.Split(' ');
            cn = st.Length;
                cn++;
                for (a = 0; a < cn; a++)
                {

                    for (b = 0; b < words; b++) if (!new string(vocab, (int)(b * max_w), (int)max_w).Equals(st[a])) break;
                    if (b == words) b = -1;
                    bi[a] = b;
                    Console.Write("\nWord: {0}  Position in vocabulary: {1}\n", st[a], bi[a]);
                    if (b == -1)
                    {
                        Console.Write("Out of dictionary word!\n");
                        break;
                    }
                }
                if (b == -1) return new BestWord[0];
                Console.WriteLine("\n                                              Word       Cosine distance\n------------------------------------------------------------------------");
                for (a = 0; a < size; a++) vec[a] = 0;
                for (b = 0; b < cn; b++)
                {
                    if (bi[b] == -1) continue;
                    for (a = 0; a < size; a++) vec[a] += M[a + bi[b] * size];
                }
                len = 0;
                for (a = 0; a < size; a++) len += vec[a] * vec[a];
                len = (float)Math.Sqrt(len);
                for (a = 0; a < size; a++) vec[a] /= len;
                //for (a = 0; a < N; a++) bestd[a] = -1;
                //for (a = 0; a < N; a++) bestw[a][0] = 0;
                for (c = 0; c < words; c++)
                {
                    a = 0;
                    for (b = 0; b < cn; b++) if (bi[b] == c) a = 1;
                    if (a == 1) continue;
                    dist = 0;
                    for (a = 0; a < size; a++) dist += vec[a] * M[a + c * size];
                    for (a = 0; a < N; a++)
                    {
                        if (dist > bestWords[a].Distance)
                        {
                            for (d = N - 1; d > a; d--)
                            {
                                //bestd[d] = bestd[d - 1];
                                //strcpy(bestw[d], bestw[d - 1]);
                                bestWords[d] = bestWords[d - 1];
                            }
                            bestWords[a].Distance = dist;
                            //bestd[a] = dist;
                            bestWords[a].Word = new string(vocab, (int)(max_w * c), (int)max_w);
                            //strcpy(bestw[a], &vocab[c * max_w]);
                            break;
                        }
                    }
                }
                //for (a = 0; a < N; a++) Console.Write("{0}\t\t{1}\n", bestw[a], bestd[a]);
            return bestWords;

        }

        private void InitVocub()
        {
            using (Stream f = File.Open(file_name, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(f))
                {
                    using (StreamReader streamReader = new StreamReader(f))
                    {
                        words = reader.ReadInt32();
                        reader.Read();
                        size = reader.ReadInt32();
                        M = new float[words * size];
                        vocab = new char[words * max_w];
                        for (b = 0; b < words; b++)
                        {
                            a = 0;
                            while (true)
                            {
                                var ch = reader.Read();
                                vocab[b * max_w + a] =  (char)ch;
                                if (streamReader.EndOfStream || (vocab[b * max_w + a] == ' ')) break;
                                if ((a < max_w) && (vocab[b * max_w + a] != '\n')) a++;
                            }
                            vocab[b * max_w + a] = '\0';
                            for (a = 0; a < size; a++) M[a + b * size] = reader.ReadSingle();//fread(&, sizeof(float), 1, f);
                            len = 0;
                            for (a = 0; a < size; a++) len += M[a + b * size] * M[a + b * size];
                            len = (float)Math.Sqrt(len);
                            for (a = 0; a < size; a++) M[a + b * size] /= len;
                        }
                    }

                }
            }
            

        }
    }

    public struct BestWord
    {
        public string Word { get; set; }
        public float Distance { get; set; }
    }
}