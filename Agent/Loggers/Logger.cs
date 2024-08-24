using Agent.Utilities;
using LivePlaylistsClone;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Loggers
{
    public class Logger
    {
        private static Logger instance = null;

        private static object _mutex = new object();

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_mutex)
                    {
                        if (instance == null)
                        {
                            instance = new Logger();
                        }
                    }
                }

                return instance;
            }
        }

        private readonly string _logsPath;
        private readonly List<Action<string>> _loggers;

        private Logger()
        {
            var commonUtilities = new CommonUtilities();

            this._logsPath = commonUtilities.CombineRoot("logs.txt");
            this._loggers = new List<Action<string>>()
            {
                this.FileLogger,
                this.ConsoleLogger
            };
        }

        private readonly object _writeLock = new object();

        public void WriteLog(string message)
        {
            lock (this._writeLock)
            {
                foreach (var logger in this._loggers)
                {
                    logger(message);
                }
            }
        }

        private void FileLogger(string message)
        {
            System.IO.File.AppendAllText(this._logsPath, message + "\n");
        }

        private void ConsoleLogger(string message)
        {
            Console.WriteLine(message);
        }
    }
}
