using System;

namespace PPC.Log
{
    public interface ILog
    {
        void Initialize(string path, string file, string fileTarget = "logfile");
        void Debug(string format, params object[] args);
        void Info(string format, params object[] args);
        void Warning(string format, params object[] args);
        void Error(string format, params object[] args);
        void Exception(Exception ex);
        void Exception(string msg, Exception ex);
        void WriteLine(LogLevels level, string format, params object[] args);
    }
}
