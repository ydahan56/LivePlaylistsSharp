﻿using Agent.Extensions;
using Agent.Loggers;
using AudDSharp;
using DotNetEnv;
using FluentScheduler;
using Hanssens.Net;
using LivePlaylistsClone.Common;
using LivePlaylistsClone.Contracts;
using LivePlaylistsSharp.Contracts;
using LivePlaylistsSharp.Models;
using Nito.AsyncEx;
using Polly;
using ShazamSharp;
using System;
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
        // music providers
        private readonly AudDClient _auddioAPI;
        private readonly ShazamClient _shazamAPI;

        private readonly ILocalStorage _storage;
        private readonly IChannel _channel;
        private readonly IPlaylist[] _playlists;

        private readonly PCManager _pcMgr;

        private readonly string _logPath;
        private readonly string _samplePath;

        private const string LAST_TRACK_KEY = "last.track";


        public Channel(IChannel channel, IPlaylist[] playlists)
        {
            // init pcmanager
            this._pcMgr = new PCManager(channel.Name);

            // inject classes
            this._channel = channel;
            this._playlists = playlists;

            // create auddio interface
            this._auddioAPI = AudDClient
                .Create(
                    Env.GetString("audd_api_token")
                )
                .IncludeProvider(StreamProvider.Apple_Music)
                .IncludeProvider(StreamProvider.Spotify)
                .Build();

            // create shazam interface
            this._shazamAPI = ShazamClient
                .Create()
                .Build();

            // setup local storage
            this._storage = new LocalStorage(
                new LocalStorageConfiguration()
                {

                    Filename = this._pcMgr.ChannelStoragePath
                }
            );

            // prepare settings
            this._logPath = this._pcMgr.ChannelLogsPath;
            this._samplePath = this._pcMgr.ChannelSamplePath;

            // monitor the channel every 4 minutes and 48 seconds,
            // to reduce the api rate limit and cost to be identical to "stream" api
            this.Schedule(this).NonReentrant().ToRunNow().AndEvery(288).Seconds();
        }

        public void Execute()
        {
            bool isSampleWritten = AsyncContext.Run(async () => await WriteChunkToFile(this._samplePath));

            if (!isSampleWritten)
            {
                // fail to write chunk to disk, so we exit and wait for next execution

                return;
            }

            var auddioFallback = Policy<IPlaylistTrack>
                .HandleResult(track => !track.Success)
                .Fallback(() =>
                {
                    // This will be executed if the shazamAPI fails
                    Console.WriteLine("Fallback to AudD Music");

                    var tack = this._auddioAPI
                        .RecognizeByFile(this._samplePath)
                        .ToTrack();

                    // if Shazam fails, we get result from auddio
                    return tack;
                });

            var track = auddioFallback.Execute(() =>
            {
                // First try to recognize with shazamAPI
                var track = Policy

                    // if we didn't find a track
                    .HandleResult<IPlaylistTrack>(track => !track.Success)

                    // or if we're out of retries
                    .OrResult(track => track.RetryTimeSpan != TimeSpan.Zero)

                    // we wait until one of the above conditions is satisfied..
                    .WaitAndRetry(
                        retryCount: int.MaxValue, 
                        sleepDurationProvider: (retryAttempt, track, context) => {

                            // default api wait time
                            return TimeSpan.FromSeconds(3);
                    })

                    // execute the retry policy
                    .Execute(() => {
                        // get track
                        var track = this._shazamAPI
                            .RecognizeByFile(this._samplePath)
                            .ToTrack();

                        // return
                        return track;
                    });

                return track;
            });

            if (!track.Success)
            {
                // print error to disk
                this.PrintLog($"{this._channel.Name} Broadcasting right now...");

                return;
            }

            if (this.TrackExistInStorage(track))
            {
                // if the same track exists in the storage, then we exit
                // reason: the track has been already written to spotify

                Thread.Sleep(30 * 1000); // todo - improve next execution logic, reduce rate limit

                return;
            }

            foreach (IPlaylist playlist in this._playlists)
            {
                AsyncContext.Run(async () => await playlist.AddTrackToPlaylistAsync(track));
            }

            // save to database
            this._storage.Store(LAST_TRACK_KEY, track);
            this._storage.Persist();

            var printTrack = new StringBuilder();
            printTrack.AppendLine(this._channel.Name);
            printTrack.AppendLine(track.ToString());

            // print track to console
            this.PrintLog(printTrack);

            // print track to file
            this.WriteLog(printTrack);

            // Mechanism to wait the leftover in order to prevent
            // the detection of the same song in different offset
            // and a redundant call to the recognition api (reduce cost)
            var tsEndGap = this.GetTrackTimeEndGap(track);

            if (tsEndGap.Ticks > 0)
            {
                var printMe = $"Next execution in... {tsEndGap.ToString("mm\\:ss")} minutes..";

                this.PrintLog(printMe);

                Thread.Sleep(tsEndGap);
            }
        }

        private bool TrackExistInStorage(IPlaylistTrack track)
        {
            if (this._storage.Exists(LAST_TRACK_KEY))
            {
                var cache_track = this._storage.Get<IPlaylistTrack>(LAST_TRACK_KEY);

                return track.Equals(cache_track);
            }

            return false;
        }

        private TimeSpan GetTrackTimeEndGap(IPlaylistTrack track)
        {
            if (track.TotalTime == TimeSpan.Zero)
            {
                return TimeSpan
                    .FromMilliseconds(210000)
                    .Subtract(track.OffsetTime);
            }

            return track.TotalTime.Subtract(track.OffsetTime);
        }

        private void WriteLog(string message)
        {
            System.IO.File.AppendAllText(this._logPath, message);
        }

        private void WriteLog(StringBuilder sb)
        {
            this.WriteLog(sb.ToString());
        }

        private void PrintLog(string message)
        {
            Console.WriteLine(message);
        }

        private void PrintLog(StringBuilder sb)
        {
            this.PrintLog(sb.ToString());
        }

        // This method saves a 128KB chunk of the steam to a local file
        private async Task<bool> WriteChunkToFile(string fileName)
        {
            try
            {
                using var http = new HttpClient();
                using var stream = await http.GetStreamAsync(new Uri(_channel.StreamUrl));
                using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);

                byte[] buffer = new byte[128000];

                // 128KB is enough to sample, 0:08 seconds length
                await stream.ReadExactlyAsync(buffer, 0, buffer.Length);
                await fileStream.WriteAsync(buffer);
            }
            catch (Exception ex)
            {
                File.AppendAllText(this._logPath, ex.ToString());

                return false;
            }

            return true;
        }
    }
}