using Agent.Contracts;
using Agent.Extensions;
using Agent.Loggers;
using Agent.Models;
using Agent.Utilities;
using AudDSharp;
using DotNetEnv;
using FluentScheduler;
using Hanssens.Net;
using LivePlaylistsClone.Contracts;
using LivePlaylistsSharp.Contracts;
using LivePlaylistsSharp.Models;
using Nito.AsyncEx;
using Polly;
using Project;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LivePlaylistsClone.Channels
{
    public class Channel : Registry, IJob
    {
        private readonly IChannel _channel;
        private readonly List<IPlaylistStrategy> _playlistStrategies;
        private readonly List<ITrackStrategy> _trackStrategies;

        private readonly CommonUtilities _commonUtilities;
        private readonly StreamUtilities _streamUtilities;
        private readonly TrackUtilities _trackUtilities;

        private int _currentStrategyIndex;
        private int _strategiesCount;

        private ITrackStrategy _currentStrategy;

        private readonly int _interval_seconds;

        public Channel(
            IChannel channel,
            List<ITrackStrategy> trackStrategies,
            List<IPlaylistStrategy> playlistStrategies
            )
        {
            // inject classes
            this._channel = channel;
            this._trackStrategies = trackStrategies;
            this._playlistStrategies = playlistStrategies;

            // setup fields
            this._commonUtilities = new CommonUtilities(channel.Name);
            this._streamUtilities = new StreamUtilities(this._commonUtilities);
            this._trackUtilities = new TrackUtilities(this._commonUtilities);

            // strategy index
            this._currentStrategyIndex = 0;
            this._strategiesCount = this._trackStrategies.Count - 1;

            // todo - revert back to 288 seconds?
            this._interval_seconds = Convert.ToInt32(
                Env.GetInt("execution_interval_sec")
            );
            this.Schedule(this).NonReentrant().ToRunNow().AndEvery(_interval_seconds).Seconds();
        }

        public void Execute()
        {
            if (!AsyncContext.Run(async () =>
            {
                var error = await this._streamUtilities.WriteChunkToFile(
                    new Uri(_channel.StreamUrl),
                    this._commonUtilities.ChannelSamplePath
                );

                return error;
            })) return;

            // todo - fuzzy algorithm-like for audio file? maybe...

            this._currentStrategy = this._trackStrategies[this._currentStrategyIndex];

            var result = AsyncContext.Run(async () =>
            {
                return await this._currentStrategy.RunAsync(
                    this._commonUtilities.ChannelSamplePath,
                    this.TrackDetectionSuccess,
                    this.TrackDetectionFail
                );
            });

            // invoke success/error callback
            result.Action(result.Track);
        }

        private void TrackDetectionSuccess(IPlaylistTrack track)
        {
            // after a match, we must reset back to defaults
            this.PerformStrategyRetryReset();
            this.PerformStrategyReset();

            if (this._trackUtilities.TrackExistInStorage(track))
            {
                // print log
                Logger.Instance.WriteLog($"{track.Title} Already exists, exiting...");

                // wait a little before next execution..
                Thread.Sleep(30 * 1000);

                return;
            }

            foreach (IPlaylistStrategy playlistStrategy in this._playlistStrategies)
            {
                AsyncContext.Run(async () => {
                    await playlistStrategy.AddTrackToPlaylistAsync(track);
                });
            }

            // commit track to storage 
            this._trackUtilities.CommitTrackDisk(track);

            var trackSb = new StringBuilder();
            trackSb.AppendLine(this._channel.Name);
            trackSb.AppendLine(track.ToString());

            Logger.Instance.WriteLog(trackSb.ToString());

            // Smart mechanism to reduce api calls to reduce costs
            if (track.IdleEnabled)
            {
                this.IdleSpin(track);
            }
        }

        private void IdleSpin(IPlaylistTrack track)
        {
            var ts = track.GetGapBetweenOffsetToEnd();

            if (ts.Ticks > 0)
            {
                var endSb = $"Next execution in... {ts:mm\\:ss} minutes..";
                Logger.Instance.WriteLog(endSb.ToString());

                // suspend thread
                Thread.Sleep(ts);
            }
        }

        private bool LastStrategyReached => 
            this._currentStrategyIndex == this._strategiesCount;

        private bool StrategyOutOfRetries =>
            this._currentStrategy.RetryCount-- == 0;

        private void PerformStrategyReset()
        {
            // reset back to first strategy
            this._currentStrategyIndex = 0;

            // print log
            Logger.Instance.WriteLog($"{this._channel.Name} - {this._currentStrategy.Name} Reset (Index: {this._currentStrategyIndex})..");
        }

        private void PerformStrategyRetryReset()
        {
            // reset current strategy retry count
            this._currentStrategy.ResetRetryCount();
        }

        private void PerformMoveNextStrategy()
        {
            // promote current strategy to next one
            this._currentStrategyIndex++;

            // print log
            var sb = new StringBuilder();
            sb.AppendLine($"{this._channel.Name} - {this._currentStrategy.Name} Couldn't find the track..");
            sb.AppendLine($"Shifting Strategy to {this._currentStrategyIndex}...");

            Logger.Instance.WriteLog(sb.ToString());
        }

        private void TrackDetectionFail(IPlaylistTrack track)
        {
            // are we out of retries? reset and move next strategy..
            if (this.StrategyOutOfRetries)
            {
                this.PerformStrategyRetryReset();

                // did we reach the last strategy?
                if (this.LastStrategyReached)
                {
                    this.PerformStrategyReset();

                    return;
                }

                this.PerformMoveNextStrategy();

                return;
            }

            // print log
            Logger.Instance.WriteLog($"{this._channel.Name} Couldn't find track, retrying again in {this._interval_seconds} seconds...");
        }
    }
}