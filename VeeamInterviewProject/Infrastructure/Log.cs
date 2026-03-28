using NLog;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Text;
using VeeamInterviewProject.Abstractions;

namespace VeeamInterviewProject.Infrastructure
{
    internal class Log : IAppLogger
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Constructor configurates log path, layout, adds Rules for logfile and console
        /// </summary>
        public Log(string logPath)
        {
            var config = new NLog.Config.LoggingConfiguration();

            var directory = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory!);
                }
                catch (Exception ex) 
                {
                    Console.WriteLine($"Error while creating directory for log: {ex.Message}");
                    throw;
                }
            }

            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = logPath,
                Layout = "${longdate} | ${level:uppercase=true} | ${message} ${exception:format=toString}",
                DeleteOldFileOnStartup = true
            };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole")
            {
                Layout = "${longdate} | ${level:uppercase=true} | ${message} ${exception:format=toString}"
            };

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Error, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            config.AddRule(LogLevel.Error, LogLevel.Fatal, logfile);

            NLog.LogManager.Configuration = config;
            logger = LogManager.GetCurrentClassLogger();
        }

        public void InfoMessage(string message)
        {
            logger.Info(message);
        }
        public void ErrorMessage(string message)
        {
            logger.Error(message);
        }
    }
}
