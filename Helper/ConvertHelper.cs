using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeerBubbleUtility
{
    public static class ConvertHelper
    {
        #region String

        public static string ToString(string str)
        {
            return str ?? string.Empty;
        }

        public static string ToString(string str, string defaultValue)
        {
            return str ?? defaultValue;
        }

        public static string ToString(object obj, string defaultValue)
        {
            return obj == null ? defaultValue : obj.ToString();
        }

        #endregion

        #region Boolean

        public static bool ToBoolean(string str)
        {
            return ToBoolean(str, false);
        }

        public static bool ToBoolean(string str, bool defaultValue)
        {
            if (str == "1") return true;

            bool result;
            return bool.TryParse(str, out result) ? result : defaultValue;
        }

        public static bool ToBoolean(object obj, bool defaultValue)
        {
            if (obj == null) return defaultValue;

            bool result;
            return bool.TryParse(obj.ToString(), out result) ? result : defaultValue;
        }

        #endregion

        #region Char

        public static char ToChar(string str)
        {
            return ToChar(str, '\0');
        }

        public static char ToChar(string str, char defaultValue)
        {
            char result;
            return char.TryParse(str, out result) ? result : defaultValue;
        }

        public static char ToChar(object obj, char defaultValue)
        {
            if (obj == null) return defaultValue;

            char result;
            return char.TryParse(obj.ToString(), out result) ? result : defaultValue;
        }

        #endregion

        #region Byte

        public static byte ToByte(string str)
        {
            return ToByte(str, 0);
        }

        public static byte ToByte(string str, byte defaultValue)
        {
            byte result;
            return byte.TryParse(str, out result) ? result : defaultValue;
        }

        public static byte ToByte(object obj, byte defaultValue)
        {
            if (obj == null) return defaultValue;

            byte result;
            return byte.TryParse(obj.ToString(), out result) ? result : defaultValue;
        }

        #endregion

        #region Int16

        public static short ToInt16(string str)
        {
            return ToInt16(str, 0);
        }

        public static short ToInt16(string str, short defaultValue)
        {
            short result;
            return short.TryParse(str, out result) ? result : defaultValue;
        }

        public static short ToInt16(object obj, short defaultValue)
        {
            if (obj == null) return defaultValue;

            short result;
            return short.TryParse(obj.ToString(), out result) ? result : defaultValue;
        }

        #endregion

        #region Int32

        public static int ToInt32(string str)
        {
            return ToInt32(str, 0);
        }

        public static int ToInt32(string str, int defaultValue)
        {
            int result;
            return int.TryParse(str, out result) ? result : defaultValue;
        }

        public static int ToInt32(object obj, int defaultValue)
        {
            if (obj == null) return defaultValue;

            int result;
            return int.TryParse(obj.ToString(), out result) ? result : defaultValue;
        }

        #endregion

        #region Int64

        public static long ToInt64(string str)
        {
            return ToInt64(str, 0);
        }

        public static long ToInt64(string str, long defaultValue)
        {
            long result;
            return long.TryParse(str, out result) ? result : defaultValue;
        }

        public static long ToInt64(object obj, long defaultValue)
        {
            if (obj == null) return defaultValue;

            long result;
            return long.TryParse(obj.ToString(), out result) ? result : defaultValue;
        }

        #endregion

        #region Float

        public static float ToFloat(string str)
        {
            return ToFloat(str, 0);
        }

        public static float ToFloat(string str, float defaultValue)
        {
            float result;
            return float.TryParse(str, out result) ? result : defaultValue;
        }

        public static float ToFloat(object obj, float defaultValue)
        {
            if (obj == null) return defaultValue;

            float result;
            return float.TryParse(obj.ToString(), out result) ? result : defaultValue;
        }

        #endregion

        #region Double

        public static double ToDouble(string str)
        {
            return ToDouble(str, 0);
        }

        public static double ToDouble(string str, double defaultValue)
        {
            double result;
            return double.TryParse(str, out result) ? result : defaultValue;
        }

        public static double ToDouble(object obj, double defaultValue)
        {
            if (obj == null) return defaultValue;

            double result;
            return double.TryParse(obj.ToString(), out result) ? result : defaultValue;
        }

        #endregion

        #region Decimal

        public static decimal ToDecimal(string str)
        {
            return ToDecimal(str, 0);
        }

        public static decimal ToDecimal(string str, decimal defaultValue)
        {
            decimal result;
            return decimal.TryParse(str, out result) ? result : defaultValue;
        }

        public static decimal ToDecimal(object obj, decimal defaultValue)
        {
            if (obj == null) return defaultValue;

            decimal result;
            return decimal.TryParse(obj.ToString(), out result) ? result : defaultValue;
        }

        #endregion

        #region DateTime

        public static DateTime ToDateTime(string str)
        {
            return ToDateTime(str, DateTime.MinValue);
        }

        public static DateTime ToDateTime(string str, DateTime defaultValue)
        {
            DateTime result;
            return DateTime.TryParse(str, out result) ? result : defaultValue;
        }

        public static DateTime ToDateTime(object obj, DateTime defaultValue)
        {
            if (obj == null) return defaultValue;

            DateTime result;
            return DateTime.TryParse(obj.ToString(), out result) ? result : defaultValue;
        }

        public static DateTime? ToDateTimeNull(object obj, DateTime? defaultValue)
        {
            if (obj == null) return defaultValue;

            DateTime result;
            return DateTime.TryParse(obj.ToString(), out result) ? result : defaultValue;
        }

        #endregion

        #region Enum

        public static T ToEnum<T>(string str)
        {
            return ToEnum(str, default(T), true);
        }

        public static T ToEnum<T>(string str, T defaultValue)
        {
            return ToEnum<T>(str, defaultValue, true);
            //if (Enum.IsDefined(typeof(T), str))
            //{
            //    return (T)Enum.Parse(typeof(T), str);
            //}

            //return defaultValue;
        }

        public static T ToEnum<T>(string str, T defaultValue, bool ignoreCase)
        {
            try
            {
                //if (Enum.IsDefined(typeof(T), str))
                //{
                return (T)Enum.Parse(typeof(T), str, ignoreCase);
                //}
            }
            catch
            {
                return defaultValue;
            }
        }

        public static T ToEnum<T>(object obj, T defaultValue)
        {
            return (obj == null) ? defaultValue : ToEnum<T>(obj.ToString(), defaultValue);
        }

        #endregion
    }
}
