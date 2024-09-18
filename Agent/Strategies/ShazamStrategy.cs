using Agent.Contracts;
using Agent.Extensions;
using Agent.Models;
using LivePlaylistsSharp.Contracts;
using Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Strategies
{
    public class ShazamStrategy : ITrackStrategy
    {
        private const int DEFAULT_RETRY_COUNT = 3;

        public ShazamStrategy()
        {
            this.Name = "Shazam";
            this.RetryCount = DEFAULT_RETRY_COUNT;
            this.RetryCounter = DEFAULT_RETRY_COUNT;
        }

        public override async Task<StrategyResult> RunAsync(
            string filePath,
            Action<IPlaylistTrack> successCallback, 
            Action<IPlaylistTrack> errorCallback
            )
        {
            using var captureHelper = new FileCaptureHelper(filePath);
            captureHelper.Start();

            var result = await CaptureAndTag.RunAsync(captureHelper);
            var track = result.ToTrack();

            if (!track.Success)
            {
                return new StrategyResult(track, errorCallback);
            }

            return new StrategyResult(track, successCallback);
        }
    }
}
