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

        private readonly object _writeLock = new object();

        public void WriteLog(string message)
        {
            lock (this._writeLock)
            {
                System.IO.File.AppendAllText(message, Environment.NewLine);
            }
        }
    }
}
