using System;
using System.IO;

namespace Agent.Utilities
{
    public class CommonUtilities
    {
        private string _channelDirectory;
        private string _workingDirectory;
        private string _channelName;

        public string ChannelStoragePath => CombineChannel($"{_channelName}.json");
        public string ChannelLogsPath => CombineChannel($"{_channelName}.txt");
        public string ChannelSamplePath => CombineChannel($"{_channelName}.mp3");

        public CommonUtilities()
        {
            _workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        public CommonUtilities(string channelName) : this()
        {
            _channelName = channelName;
            _channelDirectory = Path.Combine(_workingDirectory, channelName);

            // create channel directory if not exist
            if (!Directory.Exists(_channelDirectory))
            {
                Directory.CreateDirectory(_channelDirectory);
            }
        }

        public string CombineRoot(string fileName)
        {
            return Path.Combine(_workingDirectory, fileName);
        }

        public string CombineChannel(string fileName)
        {
            return Path.Combine(_channelDirectory, fileName);
        }
    }
}
