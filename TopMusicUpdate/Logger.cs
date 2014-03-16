using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace TopMusicUpdate
{
    public class Logger
    {
        private const string LOG_FILE_PATH = "topmusic-update.log";
        private static Encoding DEFAULT_ENCODING = Encoding.UTF8; 
        private Type type;
        private string logFilePath;
        
        public Logger(Type type)
        {
            this.type = type;
            this.logFilePath = LOG_FILE_PATH;
            if (this.logFilePath.Length < 2 || this.logFilePath.Substring(1, 1) != ":")
            {
                this.logFilePath = Application.StartupPath + (Application.StartupPath.EndsWith("\\") ? "" : "\\") + this.logFilePath;
            }
        }

        public Logger(string logFilePath)
        {
            this.logFilePath = logFilePath;
            if (this.logFilePath != null && (this.logFilePath.Length < 2 || this.logFilePath.Substring(1, 1) != ":"))
            {
                this.logFilePath = Application.StartupPath + (Application.StartupPath.EndsWith("\\") ? "" : "\\") + this.logFilePath;
            }
        }

        public Logger(string logFilePath, Type type)
        {
            this.type = type;
            this.logFilePath = logFilePath;

            if (this.logFilePath != null && (this.logFilePath.Length < 2 || this.logFilePath.Substring(1, 1) != ":"))
            {
                this.logFilePath = Application.StartupPath + (Application.StartupPath.EndsWith("\\") ? "" : "\\") + this.logFilePath;
            }
        }

        public void Debug(string message)
        {
            Debug(message, null);
        }

        public void Debug(string message, Exception ex)
        {
            StringBuilder log = new StringBuilder();
            log.Append(DateTime.Now.ToString("MM-dd HH:mm:ss")).Append(" DEBUG");
            if (this.type != null)
            {
                log.Append(" ").Append(this.type.Name);
            }

            log.Append(" - ").Append(message).AppendLine();

            if (ex != null)
            {
                log.AppendLine(ex.Source);
                log.AppendLine(ex.StackTrace);
            }

            Log(log.ToString());
        }

        public void Info(string message)
        {
            Info(message, null);
        }

        public void Info(string message, Exception ex)
        {
            StringBuilder log = new StringBuilder();
            log.Append(DateTime.Now.ToString("MM-dd HH:mm:ss")).Append(" INFO");
            if (this.type != null)
            {
                log.Append(" ").Append(this.type.Name);
            }

            log.Append(" - ").Append(message).AppendLine();

            if (ex != null)
            {
                log.AppendLine(ex.Source);
                log.AppendLine(ex.StackTrace);
            }

            Log(log.ToString());
        }

        public void Warn(string message)
        {
            Warn(message, null);
        }

        public void Warn(string message, Exception ex)
        {
            StringBuilder log = new StringBuilder();
            log.Append(DateTime.Now.ToString("MM-dd HH:mm:ss")).Append(" WARN");
            if (this.type != null)
            {
                log.Append(" ").Append(this.type.Name);
            }

            log.Append(" - ").Append(message).AppendLine();

            if (ex != null)
            {
                log.AppendLine(ex.Source);
                log.AppendLine(ex.StackTrace);
            }

            Log(log.ToString());
        }

        public void Error(string message)
        {
            Error(message, null);
        }

        public void Error(string message, Exception ex)
        {
            StringBuilder log = new StringBuilder();
            log.Append(DateTime.Now.ToString("MM-dd HH:mm:ss")).Append(" ERROR");
            if (this.type != null)
            {
                log.Append(" ").Append(this.type.Name);
            }

            log.Append(" - ").Append(message).AppendLine();

            if (ex != null)
            {
                log.AppendLine(ex.Source);
                log.AppendLine(ex.StackTrace);
            }

            Log(log.ToString());
        }

        private void Log(string message)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = File.Open(this.logFilePath, FileMode.Append, FileAccess.Write);
                byte[] bytes = DEFAULT_ENCODING.GetBytes(message);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Flush();
            }
            catch 
            { }
            finally
            {
                if (fileStream != null)
                {
                    try
                    {
                        fileStream.Close();
                    }
                    catch
                    { 
                    
                    }
                }
            }
        }
    }
}
