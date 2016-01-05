using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word2Vec.Net.Utils;

namespace Word2Vec.Net
{
    public  class Word2VecAnalysisBase
    {
        public const long max_size = 2000;         // max length of strings
        public const long N = 40;                  // number of closest words that will be shown
        public const long max_w = 50;              // max length of vocabulary entries
        private long size;
        private string file_name;
        private long words;
        private char[] vocab;
        private float[] m;
        protected char[] Vocab
        {
            get
            {
                return vocab;
            }

            set
            {
                vocab = value;
            }
        }

        protected long Words
        {
            get
            {
                return words;
            }

            set
            {
                words = value;
            }
        }

        protected long Size
        {
            get
            {
                return size;
            }

            set
            {
                size = value;
            }
        }

        protected float[] M
        {
            get
            {
                return m;
            }

            set
            {
                m = value;
            }
        }

        /// <summary>
        /// Basic class for analysis algorithms( distnace, analogy, commpute-accuracy)
        /// </summary>
        /// <param name="fileName"></param>
        public Word2VecAnalysisBase(string fileName)
        {
            file_name = fileName;           //bestw = new string[N];
            
           
            
            InitVocub();
        }

        private void InitVocub()
        {
            using (FileStream f = File.Open(file_name, FileMode.Open, FileAccess.Read))
            {
                
                //var text = ReadInt32(f);
                //int[] data = text.Split(' ').Select(int.Parse).ToArray();
                Words = f.ReadInt32();
                Size = f.ReadInt32();
                M = new float[Words*Size];
                Vocab = new char[Words*max_w];
                for (int b = 0; b < Words; b++)
                {
                    int a = 0;
                    int i = 0;
                    string word = f.ReadWord();
                    foreach (char ch in word)
                    {
                        Vocab[b*max_w + a] = ch;
                        if ((a < max_w) && (vocab[b*max_w + a] != '\n')) a++;
                    }
                    Vocab[b*max_w + a] = '\0';
                    for (a = 0; a < Size; a++)
                    {
                        byte[] bytes = new byte[4];
                        f.Read(bytes, 0, 4);
                        M[a + b*Size] = BitConverter.ToSingle(bytes, 0);
                    }
                    float len = 0;
                    for (a = 0; a < Size; a++) len += M[a + b*Size]*M[a + b*Size];
                    len = (float) Math.Sqrt(len);
                    for (a = 0; a < Size; a++) M[a + b*Size] = M[a + b*Size]/len;
                }
            }
        }





    }
}
