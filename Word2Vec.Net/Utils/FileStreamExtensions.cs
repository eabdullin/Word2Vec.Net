using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Word2Vec.Net.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public static class FileStreamExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ReadWord(this FileStream stream)
        {
            var messageBuilder = new List<byte>();

            byte byteAsInt;
            while ((byteAsInt = (byte)stream.ReadByte()) != -1)
            {
                if (byteAsInt == '\r' || byteAsInt == ' ' || stream.Position == stream.Length)
                {
                    break;
                }
                messageBuilder.Add(byteAsInt);
            }
            return Encoding.UTF8.GetString(messageBuilder.ToArray());
        }

        public static int ReadInt32( this FileStream stream)
        {
            byte[] bytes = new byte[1];
            StringBuilder builder = new StringBuilder();
            while (stream.Read(bytes, 0, 1) != -1)
            {
                if (bytes[0] == ' ' || bytes[0] == '\n' || bytes[0] == '\r') break;
                builder.Append((char)bytes[0]);
            }
            return Int32.Parse(builder.ToString());
        }
    }
}
