using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;

namespace BeerBubbleUtility
{
    enum LogType
    {
        Log,
        Error,
        Debug,
        Warn
    }

    /// <summary>
    /// Log4net兼容
    /// </summary>
    public sealed class LogHelper
    {
        private static LogHelper current = null;
        private static readonly object lockObject = new object();
        private static readonly string LOG_PATH = ConvertHelper.ToString(System.Configuration.ConfigurationManager.AppSettings["ExceptionLogPath"]);
        private static readonly bool DISABLE_LOG = ConvertHelper.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["DisableLog"]);
        private const string LOGFILE_PREFIX = "Log_";
        private const string LOG_FILENAME = "Log";
        private const string ERROR_FILENAME = "Error";
        private const string DEBUG_FILENAME = "Debug";
        private const string WARN_FILENAME = "Warn";

        #region Singleton
        private LogHelper()
        {
        }

        public static LogHelper Current
        {
            get
            {
                if (current == null)
                {
                    lock (lockObject)
                    {
                        if (current == null)
                        {
                            current = new LogHelper();
                        }
                    }
                }
                return current;
            }
        }
        #endregion

        public bool IsDebugEnabled
        {
            get
            {
#if DEBUG
                return true;
#else
				return false;
#endif
            }
        }

        public bool IsErrorEnabled
        {
            get
            {
#if DEBUG
                return true;
#else
				return false;
#endif
            }
        }

        public bool IsInfoEnabled
        {
            get
            {
#if DEBUG
                return true;
#else
				return false;
#endif
            }
        }

        public bool IsWarnEnabled
        {
            get
            {
#if DEBUG
                return true;
#else
				return false;
#endif
            }
        }

        private void Write(LogType logType, string logString)
        {
            Write(logType, string.Empty, logString);
        }

        private void Write(LogType logType, string logFileName, string logString)
        {
            string logFile = LOG_PATH + DateTime.Now.ToString("yyyy-MM-dd") + "\\";
            if (!System.IO.Directory.Exists(logFile))
            {
                System.IO.Directory.CreateDirectory(logFile);
            }
            logFile += LOGFILE_PREFIX;
            if (string.IsNullOrEmpty(logFileName))
            {
                switch (logType)
                {
                    case LogType.Log:
                        logFile += LOG_FILENAME;
                        break;
                    case LogType.Error:
                        logFile += ERROR_FILENAME;
                        break;
                    case LogType.Debug:
                        logFile += DEBUG_FILENAME;
                        break;
                    case LogType.Warn:
                        logFile += WARN_FILENAME;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("logType");
                }
            }
            else
            {
                logFile += logFileName;
            }
            string filePath = logFile + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            FileHelper.Log(filePath, logString);
        }



        public void Log(string message)
        {
            Log(string.Empty, message);
        }

        public void Log(string logFileName, string message)
        {
            if (!DISABLE_LOG)
            {
                Write(LogType.Log, logFileName, message);
            }
        }

        public void Error(Exception e)
        {
            Error(string.Empty, e);
        }

        public void Error(string errorString)
        {
            Error(errorString, null);
        }

        public void Error(string errorString, Exception e)
        {
            if (!string.IsNullOrEmpty(errorString))
            {
                Write(LogType.Error, errorString);
            }
            if (e != null)
            {
                ExceptionService.Current.Handle(e);
            }
        }

        public void Info(string infoString)
        {
            Write(LogType.Log, infoString);
        }

        public void Debug(string debugString)
        {
            Debug(debugString, null);
        }

        public void DebugFormat(string debugString, params object[] parms)
        {
            Debug(string.Format(debugString, parms), null);
        }

        public void Debug(string debugString, Exception e)
        {
            Write(LogType.Debug, debugString);
            if (e != null)
            {
                ExceptionService.Current.Handle(e);
            }
        }

        public void WarnFormat(string warnString, params object[] parms)
        {
            Debug(string.Format(warnString, parms));
        }

        public void Warn(string warnString)
        {
            Write(LogType.Warn, warnString);
        }

    }

    public class ExceptionService
    {
        private static ExceptionService current = null;
        private static object lockObject = new object();
        private static readonly string exceptionLogPath = System.Configuration.ConfigurationManager.AppSettings["ExceptionLogPath"] + "\\";

        private ExceptionService()
        {
        }

        public static ExceptionService Current
        {
            get
            {
                if (current == null)
                {
                    System.Threading.Interlocked.CompareExchange(ref current,
                                                                  new ExceptionService(), null);
                }
                return current;
            }
        }

        public void Handle(Exception e)
        {
            if (e == null)
            {
                return;
            }
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Server.ClearError();
            }
            StringBuilder sb = new StringBuilder();
            try
            {
                //以天作为目录
                string logFile = exceptionLogPath + DateTime.Now.ToString("yyyy-MM-dd") + "\\";
                if (!System.IO.Directory.Exists(logFile))
                    System.IO.Directory.CreateDirectory(logFile);
                string ExceptionName = e.GetType().Name;
                logFile = logFile + "\\" + ExceptionName;
                string FilePath = logFile + DateTime.Now.ToString("yyyyMMdd") + ".log";
                sb.Append("---------------------Header-----------------");
                sb.Append(Environment.NewLine);
                sb.Append("<" + ExceptionName + ">\n");
                sb.Append(Environment.NewLine);
                sb.Append("<LogDateTime>" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "</LogDateTime>\n");
                sb.Append(Environment.NewLine);
                sb.Append("<Message>" + HttpUtility.HtmlEncode(e.Message) + "</Message>\n");
                sb.Append(Environment.NewLine);
                if (e.Source != null)
                {
                    sb.Append("<Source>" + e.Source + "</Source>\n");
                    sb.Append(Environment.NewLine);
                }
                if (e.StackTrace != null)
                {
                    sb.Append("<StackTrace>" + e.ToString() + "</StackTrace>\n");
                    sb.Append(Environment.NewLine);
                }
                if (e.InnerException != null)
                {
                    Exception innerException = e.InnerException;
                    while (innerException != null)
                    {
                        sb.Append("<InnerException>" + HttpUtility.HtmlEncode(innerException.ToString()) + "</InnerException>\n");
                        sb.Append(Environment.NewLine);
                        innerException = innerException.InnerException;
                    }
                }
                sb.Append("<TargetSite>" + e.TargetSite + "</TargetSite>\n");
                sb.Append(Environment.NewLine);
                HttpContext context = HttpContext.Current;
                if (context != null)
                {
                    sb.Append("<Url>");
                    sb.Append(context.Request.Url == null
                                   ? string.Empty
                                   : HttpUtility.HtmlEncode(context.Request.Url.ToString()));
                    sb.Append("</Url>");
                    sb.Append(Environment.NewLine);
                    sb.Append("<OriginUrl>");
                    sb.Append(context.Request.Headers["X-Rewrite-Url"] == null
                                   ? string.Empty
                                   : HttpUtility.HtmlEncode(context.Request.Headers["X-Rewrite-Url"].ToString()));
                    sb.Append("</OriginUrl>");
                    sb.Append(Environment.NewLine);
                    sb.Append("<RawUrl>");
                    sb.Append(context.Request.RawUrl == null
                                   ? string.Empty
                                   : HttpUtility.HtmlEncode(context.Request.RawUrl.ToString()));
                    sb.Append("</RawUrl>");
                    sb.Append(Environment.NewLine);
                    sb.Append("<QueryString>");
                    sb.Append(context.Request.QueryString == null
                                   ? string.Empty
                                   : HttpUtility.HtmlEncode(context.Request.QueryString.ToString()));
                    sb.Append("</QueryString>");
                    sb.Append(Environment.NewLine);
                    sb.Append("<Form>");
                    sb.Append(context.Request.Form == null ? string.Empty : context.Request.Form.ToString());
                    sb.Append("</Form>");
                    sb.Append(Environment.NewLine);
                    sb.Append("<UrlReferrer>");
                    Uri referrer = null;
                    try
                    {
                        referrer = context.Request.UrlReferrer;
                    }
                    catch (UriFormatException)
                    {
                    }
                    sb.Append(referrer == null
                                   ? string.Empty
                                   : HttpUtility.UrlEncode(referrer.ToString()));
                    sb.Append("</UrlReferrer>");
                    sb.Append(Environment.NewLine);
                    sb.Append("<FromIp>");
                    DumpUserIp(sb);
                    sb.Append("</FromIp>");
                    sb.Append(Environment.NewLine);
                    sb.Append("<FromUA>");
                    sb.Append(context.Request.UserAgent ?? string.Empty);
                    sb.Append("</FromUA>");
                    sb.Append(Environment.NewLine);
                }
                sb.Append("</" + ExceptionName + ">\n");
                sb.Append(Environment.NewLine);
                sb.Append("---------------------Footer-----------------");
                sb.Append(Environment.NewLine);
                Write(FilePath, sb.ToString());
            }
            catch (Exception exception)
            {
                //理论上不可能进来
                try
                {
                    string filePath = FileHelper.GetTempFile("ExceptionService_");
                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                        {
                            using (TextWriter tw = TextWriter.Synchronized(sw))
                            {
                                tw.WriteLine(exception.Message);
                                tw.WriteLine(sb.ToString());
                                tw.Close();
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }

        static void DumpUserIp(StringBuilder builder)
        {
            HttpContext current = HttpContext.Current;
            if (current != null)
            {
                string userIp = current.Request.UserHostAddress;
                if (!string.IsNullOrEmpty(userIp))
                {
                    builder.AppendFormat("UserHostAddress:{0},", userIp);
                }
                //
                string proxyIp = current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!string.IsNullOrEmpty(proxyIp))
                {
                    builder.AppendFormat("HTTP_X_FORWARDED_FOR:{0},", proxyIp);
                }
                //
                string haProxyIp = current.Request.Headers["X-Forwarded-For"];
                if (!string.IsNullOrEmpty(haProxyIp))
                {
                    builder.AppendFormat("X-Forwarded-For:{0},", haProxyIp);
                }
                //
                string netscalerIp = current.Request.Headers["X-Proxy-Real-IP"];
                if (!string.IsNullOrEmpty(netscalerIp))
                {
                    builder.AppendFormat("X-Proxy-Real-IP:{0},", netscalerIp);
                }
                //
                string cdnIp = current.Request.Headers["Cdn-Src-Ip"];
                if (!string.IsNullOrEmpty(cdnIp))
                {
                    builder.AppendFormat("Cdn-Src-Ip:{0},", cdnIp);
                }
                //
                string isapiIp = current.Request.Headers["X-Real-IP"];
                if (!string.IsNullOrEmpty(isapiIp))
                {
                    builder.AppendFormat("X-Real-IP:{0},", isapiIp);
                }
            }
        }

        #region 写信息

        /// <summary>
        /// 写异常日志
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="Message">日志信息</param>
        private static void Write(string filePath, string Message)
        {
            FileHelper.Write(filePath, Message);
        }

        #endregion

        #region 判断目录是否存在并创建

        /// <summary>
        /// 检查一个目录是不是存在，如果不存在就创建
        /// </summary>
        /// <param name="filePath"></param>
        private static void CheckDir(ref string filePath)
        {
            if (filePath != "")
            {
                filePath = filePath.Replace("/", "\\");
                string strFileDir = filePath.Substring(0, filePath.LastIndexOf("\\"));
                //检查目录是否存在，不存在创建此目录
                if (!Directory.Exists(strFileDir))
                {
                    Directory.CreateDirectory(strFileDir);
                }
            }
            else
                return;
        }

        #endregion
    }
}
