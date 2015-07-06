using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeerBubbleUtility
{
    public static class IDHelper
    {
        private const string ValidChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_-";

        public static string NewID()
        {
            return NewID(DateTime.Now);
        }

        public static string NewID(DateTime dt)
        {
            return string.Format("{0}{1}", dt.ToString("yyMMddHHmmssfff"),
                Guid.NewGuid().ToString("N").Substring(0, 21).ToUpper());
        }

        public static string NewActivityOrderID()
        {
            return string.Format("{0}{1}", DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                new Random().Next(0, 1000000).ToString("000000"));
        }

        /// <summary>
        /// 获取支付订单号，目前暂定23位数字
        /// </summary>
        /// <returns></returns>
        public static string NewPaymentID()
        {
            return string.Format("{0}{1}", DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                new Random().Next(0, 1000000).ToString("000000"));
        }

        public static string NewShortID()
        {
            return DateTime.Now.ToString("yyMMddHHmmssfff") + new Random().Next(0, 1000).ToString("000");
        }

        public static string LongToString64(long value)
        {
            StringBuilder builder = new StringBuilder();
            while (value > 0)
            {
                int remainder = (int)(value % 64);  // 余数
                builder.Insert(0, ValidChars[remainder]);
                value /= 64;
            }

            return builder.ToString();
        }

        public static long String64ToLong(string str)
        {
            long result = 0;
            for (int i = 0; i < str.Length; i++)
            {
                result += ValidChars.IndexOf(str[i]) * (long)Math.Pow(64, str.Length - i - 1);
            }

            return result;
        }

    }
}
