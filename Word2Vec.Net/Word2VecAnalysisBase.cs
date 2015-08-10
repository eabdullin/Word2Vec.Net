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
                using (BinaryReader reader = new BinaryReader(f))
                {
                    using (StreamReader streamReader = new StreamReader(f))
                    {
                        var text = streamReader.ReadLine();
                        int[] data = text.Split(' ').Select(int.Parse).ToArray();
                        Words = data[0];
                        Size = data[1];
                        M = new float[Words * Size];
                        Vocab = new char[Words * max_w];
                        for (int b = 0; b < Words; b++)
                        {
                            int a = 0;
                            var i = 0;
                            //string word = ReadString(reader);
                            while (true)
                            {
                                
                                char ch = (char)streamReader.Read();
                                Vocab[b * max_w + a] = ch;
                                if (streamReader.EndOfStream || (Vocab[b * max_w + a] == ' ')) break;
                                if ((a < max_w) && (Vocab[b*max_w + a] != '\n'))
                                {
                                    
                                    a++;
                                }
                                   
                            }
                            Vocab[b * max_w + a] = '\0';
                            for (a = 0; a < Size; a++) M[a + b*Size] = reader.ReadSingle();
                            
                            double len = 0;
                            for (a = 0; a < Size; a++) len += M[a + b * Size] * M[a + b * Size];
                            len = Math.Sqrt(len);
                            for (a = 0; a < Size; a++) M[a + b * Size] = (float)(M[a + b * Size]/len);
                        }
                    }

                }
            }
        }

        string ReadString(BinaryReader reader)
        {
            var messageBuilder = new List<byte>();

            byte byteAsInt;
            while ((byteAsInt = reader.ReadByte()) != -1)
            {
                if (byteAsInt == '\r' || byteAsInt == ' ' || reader.BaseStream.Position == reader.BaseStream.Length)
                {
                    break;
                }
                messageBuilder.Add(byteAsInt);
            }
            return Encoding.Default.GetString(messageBuilder.ToArray());
        }
        private string ReadString(BinaryReader reader, int length = 260)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; ++i)
            {
                byte b = reader.ReadByte();
                sb.Append((char)b);
                if (reader.BaseStream.Position != reader.BaseStream.Length || b == ' ') break;
            }
            return sb.ToString().Trim();
        }
    }
}
