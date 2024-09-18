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
        private readonly StorageUtilities _storageUtilities;

        // Current index of selected strategy
        private int CurrentStrategyIndex;

        // Total amount of strategies available
        private int TotalStrategiesCount;

        // Current strategy implementation
        private ITrackStrategy CurrentStrategy;
        
        // execution delay in seconds
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
            this._storageUtilities = new StorageUtilities(this._commonUtilities);

            // strategy index
            this._storageUtilities.TryGetLastStrategyIndex(
                out this.CurrentStrategyIndex
            );

            // todo - trygetlastretrycount

            this.TotalStrategiesCount = this._trackStrategies.Count - 1;

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
                var error = await this._streamUtilities.WriteChunkToFileAsync(
                    new Uri(_channel.StreamUrl),
                    this._commonUtilities.ChannelSamplePath
                );

                return error;
            })) return;

            // todo - fuzzy-like algorithm for audio file? maybe...

            this.CurrentStrategy = this._trackStrategies[this.CurrentStrategyIndex];

            var result = AsyncContext.Run(async () =>
            {
                return await this.CurrentStrategy.RunAsync(
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

            if (this._storageUtilities.StorageTrackEquals(track))
            {
                // print log
                Logger.Instance.WriteLog($"{this._channel.Name} - {track.Title} Already exists, exiting...");

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
            this._storageUtilities.CommitTrackDisk(track);

            var trackSb = new StringBuilder();
            trackSb.AppendLine(this._channel.Name);
            trackSb.AppendLine(track.ToString());

            Logger.Instance.WriteLog(trackSb.ToString());

            // Smart mechanism to reduce api calls to reduce costs
            if (track.IdleEnabled)
            {
                this.SpinIdle(track);
            }
        }

        private void SpinIdle(IPlaylistTrack track)
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
            this.CurrentStrategyIndex == this.TotalStrategiesCount;

        private void PerformStrategyReset()
        {
            // reset back to first strategy
            this.CurrentStrategyIndex = 0;

            // print log
            Logger.Instance.WriteLog($"{this._channel.Name} - {this.CurrentStrategy.Name} Reset ( Index: {this.CurrentStrategyIndex} )..");
        }

        private void PerformStrategyRetryReset()
        {
            // reset current strategy retry count
            this.CurrentStrategy.ResetCounter();
        }

        private void PerformMoveNextStrategy()
        {
            // promote current strategy to next one
            this.CurrentStrategyIndex++;

            // print log
            var sb = new StringBuilder();
            sb.AppendLine($"{this._channel.Name} - {this.CurrentStrategy.Name} Couldn't find the track..");
            sb.AppendLine($"Shifting next Strategy... ( Index: {this.CurrentStrategyIndex} )");

            Logger.Instance.WriteLog(sb.ToString());
        }

        private void TrackDetectionFail(IPlaylistTrack track)
        {
            // are we out of retries? reset and move next strategy..
            if (!this.CurrentStrategy.HasRetries())
            {
                // the strategy will reset its counter when returning false;
                // ...

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