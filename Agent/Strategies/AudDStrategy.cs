using Agent.Contracts;
using Agent.Extensions;
using Agent.Models;
using AudDSharp;
using DotNetEnv;
using LivePlaylistsSharp.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Strategies
{
    public class AudDStrategy : ITrackStrategy
    {
        private readonly AudDClient _auddio;
        private const int DEFAULT_RETRY_COUNT = 1;

        public AudDStrategy()
        {
            this.Name = "AudD Music";
            this.RetryCount = DEFAULT_RETRY_COUNT;

            this._auddio = AudDClient
                .Create(
                    Env.GetString("audd_api_token")
                )
                .IncludeProvider(StreamProvider.Apple_Music)
                .IncludeProvider(StreamProvider.Spotify)
                .Build();
        }

        public override void ResetRetryCount()
        {
            this.RetryCount = DEFAULT_RETRY_COUNT;
        }

        public override async Task<StrategyResult> RunAsync(
            string filePath,
            Action<IPlaylistTrack> successCallback,
            Action<IPlaylistTrack> errorCallback
            )
        {

            var result = await this._auddio.RecognizeByFileAsync(filePath);
            var track = result.ToTrack();

            if (!track.Success)
            {
                return new StrategyResult(track, errorCallback);
            }

            return new StrategyResult(track, successCallback);
        }
    }
}
