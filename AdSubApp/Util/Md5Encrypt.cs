using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace AdSubApp.Util
{
    /// <summary>
    /// MD5加密
    /// </summary>
    public class Md5Encrypt
    {
        /// <summary>
        /// 输出MD5值
        /// </summary>
        /// <param name="inputString">字符串</param>
        /// <param name="encoding">编码方式</param>
        public string Output(string inputString, Encoding encoding)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5 = MD5.Create();

            // Compute the hash.
            byte[] bHashValue = md5.ComputeHash(encoding.GetBytes(inputString));

            // Create a new Stringbuilder to collect the bytes
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < bHashValue.Length; i++)
            {
                sBuilder.Append(bHashValue[i].ToString("x2").ToUpper());
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// 输出MD5值
        /// </summary>
        /// <param name="inputBuffer">二进制数据</param>
        public string Output(byte[] inputBuffer)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5 = MD5.Create();

            // Compute the hash.
            byte[] bHashValue = md5.ComputeHash(inputBuffer);

            // Create a new Stringbuilder to collect the bytes
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < bHashValue.Length; i++)
            {
                sBuilder.Append(bHashValue[i].ToString("x2").ToUpper());
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// 输出MD5值
        /// </summary>
        /// <param name="inputStream">数据流</param>
        public string Output(Stream inputStream)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5 = MD5.Create();

            // Compute the hash.
            byte[] bHashValue = md5.ComputeHash(inputStream);

            // Create a new Stringbuilder to collect the bytes
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < bHashValue.Length; i++)
            {
                sBuilder.Append(bHashValue[i].ToString("x2").ToUpper());
            }
            return sBuilder.ToString();
        }
    }
}