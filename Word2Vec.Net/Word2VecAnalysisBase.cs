using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private double[] m;
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

        protected double[] M
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
            using (Stream f = File.Open(file_name, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(f, Encoding.UTF8))
                {
                    using (StreamReader streamReader = new StreamReader(f))
                    {
                        Words = reader.ReadInt32();
                        reader.Read();
                        Size = reader.ReadInt32();
                        M = new double[Words * Size];
                        Vocab = new char[Words * max_w];
                        for (int b = 0; b < Words; b++)
                        {
                            int a = 0;
                            var i = 0;
                            string word = ReadUtfWord(reader);
                            foreach (var ch in word)
                            {
                                Vocab[b * max_w + a] = ch;
                                a++;
                            }
                            //while (true)
                            //{


                            //    if (streamReader.EndOfStream || (vocab[b * max_w + a] == ' ')) break;
                            //    if ((a < max_w) && (vocab[b * max_w + a] != '\n')) a++;
                            //}
                            Vocab[b * max_w + a] = '\0';
                            for (a = 0; a < Size; a++) M[a + b * Size] = reader.ReadDouble();//fread(&, sizeof(float), 1, f);
                            double len = 0;
                            for (a = 0; a < Size; a++) len += M[a + b * Size] * M[a + b * Size];
                            len = Math.Sqrt(len);
                            for (a = 0; a < Size; a++) M[a + b * Size] /= len;
                        }
                    }

                }
            }
        }

        string ReadUtfWord(BinaryReader reader)
        {
            var messageBuilder = new List<byte>();

            int byteAsInt;
            string result = null;
            while ((byteAsInt = reader.ReadByte()) != -1)
            {
                messageBuilder.Add((byte)byteAsInt);

                if (byteAsInt == '\r' || byteAsInt == ' ')
                {
                    result = Encoding.UTF8.GetString(messageBuilder.ToArray()).Replace("\n", "").Replace(" ", ""); ;
                    break;
                }
            }
            return result;
        }
    }
}
