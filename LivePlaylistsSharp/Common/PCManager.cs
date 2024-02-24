using System;
using System.IO;

namespace LivePlaylistsClone.Common
{
    public class PCManager
    {
        private string _channelDirectory;
        private string _workingDirectory;

        public PCManager(string channelName)
        {
            this._workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            this._channelDirectory = Path.Combine(this._workingDirectory, channelName);

            // create channel directory if not exist
            if (!Directory.Exists(_channelDirectory))
            {
                Directory.CreateDirectory(_channelDirectory);
            }
        }

        public string CombineRoot(string fileName)
        {
            return Path.Combine(this._workingDirectory, fileName);
        }

        public string CombineChannel(string fileName)
        {
            return Path.Combine(this._channelDirectory, fileName);
        }
    }
}
