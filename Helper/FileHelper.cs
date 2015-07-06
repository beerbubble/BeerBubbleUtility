using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace BeerBubbleUtility
{
    /// <summary>
    /// 文件操作类
    /// </summary>
    public static class FileHelper
    {
        private static readonly string exceptionLogPath = System.Configuration.ConfigurationManager.AppSettings["ExceptionLogPath"];

        public static void Log(string filePhysicalPath, string text)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("///---Begin(");
            stringBuilder.Append(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"));
            stringBuilder.Append(")---///");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(text);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("///----End----///");
            stringBuilder.Append(Environment.NewLine);
            Write(filePhysicalPath, stringBuilder.ToString());
        }

        public static void Write(string filePhysicalPath, string text)
        {
            SaveFileText(text, filePhysicalPath, false, true);
        }

        public static void SaveFileText(string content, string filePath, bool gb2312, bool append)
        {
            SaveFileText(content, filePath, gb2312 ? Encoding.GetEncoding("gb2312") : Encoding.UTF8, append);
        }

        public static void SaveFileText(string content, string file, Encoding encoding, bool append)
        {
            string[] filepaths = file.IndexOf('|') > 0 ? file.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries) : new[] { file };
            foreach (string filepath in filepaths)
            {
                CheckDirectory(filepath);
                int retries = 3;
                int msecsBetweenRetries = 1000;
                while (retries > 0)
                {
                    try
                    {
                        using (
                            FileStream fs = new FileStream(filepath, append ? FileMode.Append : FileMode.Create, FileAccess.Write,
                                                           FileShare.ReadWrite))
                        {
                            using (StreamWriter sw = new StreamWriter(fs, encoding))
                            {
                                using (TextWriter tw = TextWriter.Synchronized(sw))
                                {
                                    tw.Write(content);
                                    tw.Close();
                                }
                            }
                        }
                    }
                    catch
                    {
                        retries--;
                        Thread.Sleep(msecsBetweenRetries);
                        continue;
                    }
                    break;
                }
                if (retries == 0)
                {
                    string tempFilePath = GetTempFile();
                    SaveFileTextNoWait(
                        string.Format("Error SaveFileText {0}, perhaps it is in use by another process?\r\n{1}", filepath, content),
                        tempFilePath);
                }
            }

        }

        public static string GetTempFile()
        {
            return GetTempFile(string.Empty);
        }

        public static string GetTempFile(string prefix)
        {
            string logFile = exceptionLogPath + DateTime.Now.ToString("yyyy-MM-dd") + "\\";
            if (!System.IO.Directory.Exists(logFile))
            {
                System.IO.Directory.CreateDirectory(logFile);
            }
            string ExceptionName = prefix + IDHelper.NewID().ToString();
            logFile = logFile + ExceptionName;
            return logFile + ".log";
        }

        public static void SaveFileTextNoWait(string content, string filePath)
        {
            SaveFileTextNoWait(content, filePath, false, false);
        }

        public static void SaveFileTextNoWait(string content, string filePath, bool gb2312, bool append)
        {
            CheckDirectory(filePath);
            try
            {
                using (FileStream fs = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(fs, gb2312 ? Encoding.GetEncoding("gb2312") : Encoding.UTF8))
                    {
                        using (TextWriter tw = TextWriter.Synchronized(sw))
                        {
                            tw.Write(content);
                            tw.Close();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public static void CheckDirectory(string filePath)
        {
            string path = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        #region 保存二进制文件内容
        /// <summary>
        /// 保存二进制文件内容
        /// </summary>
        /// <param name="content">要保存的内容</param>
        /// <param name="filePath">文件路径</param>
        public static void SaveFileBinary(byte[] content, string file)
        {
            string[] filepaths = file.IndexOf('|') > 0 ? file.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries) : new[] { file };
            //
            foreach (string filepath in filepaths)
            {
                CheckDirectory(filepath);
                using (FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(content);
                        //for ( int i = 0, count = content.Length; i < count; i++ )
                        //{
                        //    bw.Write ( content [i] );
                        //}
                    }
                }
            }

        }
        #endregion

        #region 生成日期的文件名
        private static string GenerateFilename()
        {
            return DateTime.Now.ToString("HHmmssffff") + "." + new Random().Next(10000000, 99999999);
        }

        private static string GenerateFilepath()
        {
            DateTime now = DateTime.Now;
            return now.Year.ToString("0000") + "/" + now.Month.ToString("00") + "/" + now.Day.ToString("00") + "/";
        }

        public static string GenerateDateFilename()
        {
            return GenerateFilepath() + GenerateFilename();
        }
        #endregion
    }
}
