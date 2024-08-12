using System;
using System.IO;

namespace LivePlaylistsClone.Common
{
    public class PCManager
    {
        private string _channelDirectory;
        private string _workingDirectory;
        private string _channelName;

        public string ChannelStoragePath => this.CombineChannel($"{this._channelName}.json");
        public string ChannelLogsPath => this.CombineChannel($"{this._channelName}.txt");
        public string ChannelSamplePath => this.CombineChannel($"{this._channelName}.mp3");

        public PCManager(string channelName)
        {
            this._channelName = channelName;

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
