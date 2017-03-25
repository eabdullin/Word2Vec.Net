using System;
using System.Text;

namespace Word2Vec.Net
{
  public static class StringExtension
  {
    public static byte[] GetBytes(this string str)
    {
      return Encoding.UTF8.GetBytes(str);
    }

    public static string GetString(this byte[] bytes)
    {
      var chars = new char[bytes.Length / sizeof(char)];
      Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
      return new string(chars);
    }
  }
}