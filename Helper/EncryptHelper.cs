using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BeerBubbleUtility
{
    /// <summary>
    /// EncryptHelper 的摘要说明。
    /// </summary>
    public static class EncryptHelper
    {
        #region Fields

        private static readonly byte[] _DESKeyBytes = { 0xf3, 0x91, 0xd6, 0xff, 0x32, 0x1f, 0x4a, 0x02 };
        private static readonly byte[] _DESIVBytes = { 0x15, 0x33, 0x21, 0x9f, 0xb6, 0xdc, 0xaf, 0xd8 };
        private static readonly byte[] _AESKeyBytes = { 0xf3, 0x91, 0xd6, 0xff, 0x32, 0x1f, 0x4a, 0x02, 0xe4, 0x88, 0x25, 0x90, 0x72, 0xb3, 0xa7, 0x11 };
        private static readonly byte[] _AESIVBytes = { 0x15, 0x33, 0x21, 0x9f, 0xb6, 0xdc, 0xaf, 0xd8, 0xd4, 0x37, 0x5f, 0x95, 0x13, 0xe4, 0x72, 0xdd };
        private const int SALT_BYTES_LENGTH = 16;   // 盐值长度

        #endregion

        #region AES 加密/解密

        /// <summary>
        /// AES 加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string AESEncrypt(string input)
        {
            return AESEncrypt(input, _AESKeyBytes, _AESIVBytes);
        }

        /// <summary>
        /// DES 加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string AESEncrypt(string input, byte[] key, byte[] iv)
        {
            RijndaelManaged rm = new RijndaelManaged();
            using (ICryptoTransform ct = rm.CreateEncryptor(key, iv))
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                cs.Write(bytes, 0, bytes.Length);
                cs.Write(GetSaltBytes(), 0, SALT_BYTES_LENGTH); // 增加随即生成的盐值，保证每次加密的结果不同
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// DES 解密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string AESDecrypt(string input)
        {
            return AESDecrypt(input, _AESKeyBytes, _AESIVBytes);
        }

        /// <summary>
        /// DES 解密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string AESDecrypt(string input, byte[] key, byte[] iv)
        {
            RijndaelManaged rm = new RijndaelManaged();
            using (ICryptoTransform ct = rm.CreateDecryptor(key, iv))
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write))
            {
                byte[] bytes = Convert.FromBase64String(input);
                cs.Write(bytes, 0, bytes.Length);
                cs.FlushFinalBlock();

                // 去除盐值部分
                bytes = ms.ToArray();
                byte[] result = new byte[bytes.Length - SALT_BYTES_LENGTH];
                Buffer.BlockCopy(bytes, 0, result, 0, result.Length);

                return Encoding.UTF8.GetString(result);
            }
        }

        #endregion

        #region DES 加密/解密

        /// <summary>
        /// DES 加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string DESEncrypt(string input)
        {
            return DESEncrypt(input, _DESKeyBytes, _DESIVBytes);
        }

        /// <summary>
        /// DES 加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string DESEncrypt(string input, byte[] key, byte[] iv)
        {
            DESCryptoServiceProvider csp = new DESCryptoServiceProvider();
            using (ICryptoTransform ct = csp.CreateEncryptor(key, iv))
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                cs.Write(bytes, 0, bytes.Length);
                cs.Write(GetSaltBytes(), 0, SALT_BYTES_LENGTH); // 增加随即生成的盐值，保证每次加密的结果不同
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// DES 解密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string DESDecrypt(string input)
        {
            return DESDecrypt(input, _DESKeyBytes, _DESIVBytes);
        }

        /// <summary>
        /// DES 解密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string DESDecrypt(string input, byte[] key, byte[] iv)
        {
            DESCryptoServiceProvider csp = new DESCryptoServiceProvider();
            using (ICryptoTransform ct = csp.CreateDecryptor(key, iv))
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write))
            {
                byte[] bytes = Convert.FromBase64String(input);
                cs.Write(bytes, 0, bytes.Length);
                cs.FlushFinalBlock();

                // 去除盐值部分
                bytes = ms.ToArray();
                byte[] result = new byte[bytes.Length - SALT_BYTES_LENGTH];
                Buffer.BlockCopy(bytes, 0, result, 0, result.Length);

                return Encoding.UTF8.GetString(result);
            }
        }

        #endregion

        #region Hash 加密

        /// <summary> Hash 加密 </summary>
        /// <param name="str2Hash"></param>
        /// <returns></returns>
        public static int HashEncrypt(string str2Hash)
        {
            const int salt = 791016;    // 盐值
            str2Hash += "noname";       // 增加一个常量字符串

            int len = str2Hash.Length;
            int result = (str2Hash[len - 1] - 31) * 95 + salt;
            for (int i = 0; i < len - 1; i++)
            {
                result = (result * 95) + (str2Hash[i] - 32);
            }

            return result;
        }

        #endregion

        #region MD5

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input">待加密字串</param>
        /// <returns>加密后的字串</returns>
        public static string MD5Encrypt(string input)
        {
            return MD5Encrypt(input, 0);
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input">待加密字串</param>
        /// <param name="length">16或32值之一，其它则采用.net默认MD5加密算法</param>
        /// <returns>加密后的字串</returns>
        public static string MD5Encrypt(string input, int length)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(input);
            byte[] hashValue = MD5.Create().ComputeHash(buffer);

            string result;
            switch (length)
            {
                case 16:
                    result = BitConverter.ToString(hashValue, 4, 8).Replace("-", "");
                    break;
                case 32:
                    result = BitConverter.ToString(hashValue, 0, 16).Replace("-", "");
                    break;
                default:
                    result = BitConverter.ToString(hashValue).Replace("-", "");
                    break;
            }

            return result;
        }


        /// <summary>
        /// 获取MD5值（小写形式）
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string MD5_Lower(string input, Encoding encoding)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(encoding.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        #endregion

        #region Common

        private static byte[] GetSaltBytes()
        {
            byte[] bytes = new byte[SALT_BYTES_LENGTH];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);

            return bytes;
        }

        #endregion


    }
}