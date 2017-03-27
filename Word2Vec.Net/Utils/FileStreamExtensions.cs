using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Word2Vec.Net.Utils
{
  /// <summary>
  /// </summary>
  public static class FileStreamExtensions
  {
    public static int ReadInt32(this FileStream stream)
    {
      var bytes = new byte[1];
      var builder = new StringBuilder();
      while (stream.Read(bytes, 0, 1) != -1)
      {
        if (bytes[0] == ' ' || bytes[0] == '\n' || bytes[0] == '\r')
          break;
        builder.Append((char) bytes[0]);
      }
      return int.Parse(builder.ToString());
    }

    /// <summary>
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static string ReadWord(this FileStream stream)
    {
      var messageBuilder = new List<byte>();

      byte byteAsInt;
      while ((byteAsInt = (byte) stream.ReadByte()) != -1)
      {
        if (byteAsInt == '\r' || byteAsInt == ' ' || stream.Position == stream.Length)
          break;
        messageBuilder.Add(byteAsInt);
      }
      return Encoding.UTF8.GetString(messageBuilder.ToArray());
    }
  }
}