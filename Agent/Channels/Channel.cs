using Agent.Extensions;
using AudDSharp;
using DotNetEnv;
using FluentScheduler;
using Hanssens.Net;
using LivePlaylistsClone.Common;
using LivePlaylistsClone.Contracts;
using LivePlaylistsSharp.Models;
using Nito.AsyncEx;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LivePlaylistsClone.Channels
{
    public class Channel : Registry, IJob
    {
        private readonly AudDClient _auddio;
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

            // init audd.io
            this._auddio = AudDClient
                .Create(
                    Env.GetString("audd_api_token")
                )
                .IncludeProvider(StreamProvider.Apple_Music)
                .IncludeProvider(StreamProvider.Spotify)
                .Build();

            // setup local storage
            this._storage = new LocalStorage(
                new LocalStorageConfiguration()
                {

                    Filename = this._pcMgr.CombineChannel($".{channel.Name}")
                }
            );

            // injected classes
            this._channel = channel;
            this._playlists = playlists;

            // prepare settings
            this._logPath = this._pcMgr.CombineChannel("log.txt");
            this._samplePath = this._pcMgr.CombineChannel("sample.mp3");

            // schedules
            this.Schedule(this).NonReentrant().ToRunNow().AndEvery(30).Seconds();
        }

        public void Execute()
        {
            bool isSampleWritten = AsyncContext.Run(async () => await WriteChunkToFile(this._samplePath));

            if (!isSampleWritten)
            {
                // fail to write chunk to disk, so we exit and wait for next execution

                return;
            }

            var track = this._auddio
                .RecognizeByFile(this._samplePath)
                .ToTrack();

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
                AsyncContext.Run(async () =>
                {
                    await playlist.AddTrackToPlaylistAsync(track);
                });
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
            var tsRemainder = this.GetNextTrackRemainder(track);

            // print remainder
            this.PrintLog(tsRemainder.ToString("mm\\:ss"));

            // we call Duration() to convert the TimeSpan to absolute value
            var tsWait = tsRemainder
                .Subtract(track.SegmentOffset)
                .Duration();

            if (tsWait.Ticks > 0)
            {
                var printMe = $"Next execution in... {tsWait.ToString("mm\\:ss")} minutes";

                this.PrintLog(printMe);

                Thread.Sleep(tsWait);
            }
        }

        private bool TrackExistInStorage(AudDTrack track)
        {
            if (this._storage.Exists(LAST_TRACK_KEY))
            {
                var cache_track = this._storage.Get<AudDTrack>(LAST_TRACK_KEY);

                return track.Equals(cache_track);
            }

            return false;
        }

        private TimeSpan GetNextTrackRemainder(AudDTrack track)
        {
            return string.IsNullOrWhiteSpace(track.ProviderUri) ?
                TimeSpan.FromMinutes(4) :
                track.SegmentOffset;
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