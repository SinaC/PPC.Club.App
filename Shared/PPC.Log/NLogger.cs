using System;
using System.IO;
using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace PPC.Log
{
    public class NLogger : ILog
    {
        private readonly Logger _logger;

        public NLogger(string loggerName)
        {
            if (loggerName == null)
                throw new ArgumentNullException(nameof(loggerName));
            _logger = LogManager.GetLogger(loggerName);
        }

        #region ILog

        public void Initialize(string path, string file, string fileTargetName = "logfile")
        {
            //
            string logfile = Path.Combine(path, file);
            //FileTarget target = LogManager.Configuration.FindTargetByName(fileTarget) as FileTarget;
            var target = LogManager.Configuration.FindTargetByName(fileTargetName);
            if (target == null)
                throw new ApplicationException($"Couldn't find target {fileTargetName} in NLog config");
            FileTarget fileTarget = null;
            if (target is AsyncTargetWrapper)
                fileTarget = ((AsyncTargetWrapper)target).WrappedTarget as FileTarget;
            else
                fileTarget = target as FileTarget;
            if (fileTarget == null)
                throw new ApplicationException($"Target {fileTargetName} is not a FileTarget");
            fileTarget.FileName = logfile;
        }

        public void Debug(string format, params object[] args)
        {
            _logger.Debug(format, args);
        }

        public void Info(string format, params object[] args)
        {
            _logger.Info(format, args);
        }

        public void Warning(string format, params object[] args)
        {
            _logger.Info(format, args);
        }

        public void Error(string format, params object[] args)
        {
            _logger.Error(format, args);
        }

        public void Exception(Exception ex)
        {
            _logger.Error(ex, "Exception");
        }

        public void Exception(string msg, Exception ex)
        {
            _logger.Error(ex, msg);
        }

        public void WriteLine(LogLevels level, string format, params object[] args)
        {
            switch (level)
            {
                case LogLevels.Debug:
                    _logger.Debug(format, args);
                    break;
                case LogLevels.Info:
                    _logger.Info(format, args);
                    break;
                case LogLevels.Warning:
                    _logger.Warn(format, args);
                    break;
                case LogLevels.Error:
                    _logger.Error(format, args);
                    break;
            }
        }

        #endregion
    }
}
