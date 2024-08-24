using Agent.Models;
using LivePlaylistsSharp.Contracts;
using Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Contracts
{
    public abstract class ITrackStrategy
    {
        public string Name { get; protected set; }
        public int RetryCount { get; set; }

        public abstract void ResetRetryCount();

        public abstract Task<StrategyResult> RunAsync(
            string filePath,
            Action<IPlaylistTrack> successCallback,
            Action<IPlaylistTrack> errorCallback
        );
    }
}
