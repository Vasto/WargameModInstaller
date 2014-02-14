using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.ViewModels;

namespace WargameModInstaller.Common.Logging
{
    public class Logger : ILog
    {
        private readonly String logFileFullPath;
        private readonly Type type;

        public Logger()
        {
            this.logFileFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
        }

        public Logger(Type type)
        {
            this.logFileFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
            this.type = type;
        }

        public void Error(Exception exception)
        {
            using (StreamWriter writer = File.AppendText(logFileFullPath))
            {
                LogException(writer, "ERROR", exception);
            }
        }

        public void Info(String format, params object[] args)
        {
            using (StreamWriter writer = File.AppendText(logFileFullPath))
            {
                Log(writer, "INFO", format, args);
            }
        }

        public void Warn(String format, params object[] args)
        {
            using (StreamWriter writer = File.AppendText(logFileFullPath))
            {
                Log(writer, "WARNING", format, args);
            }
        }

        private void LogException(StreamWriter writer, String msgType, Exception ex)
        {
            writer.WriteLine(String.Format("{0}: {1} {2}: ", msgType, DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString()));
            writer.WriteLine(String.Format(ex.ToString()));
            writer.WriteLine("-----------------------------------------------------");
            writer.WriteLine();

        }

        private void Log(StreamWriter writer, String msgType, String format, params object[] args)
        {
            writer.WriteLine(String.Format("{0}: {1} {2}: ", msgType, DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString()));
            writer.WriteLine(String.Format(format, args));
            writer.WriteLine("-----------------------------------------------------");
            writer.WriteLine();

        }

    }

}
