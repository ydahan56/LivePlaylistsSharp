using AudDSharp;
using AudDSharp.Models;
using DotNetEnv;
using FluentScheduler;
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
        private readonly IChannel _channel;
        private readonly IPlaylist[] _playlists;

        private readonly PCManager _pcMgr;

        private readonly string _logPath;
        private readonly string _samplePath;


        public Channel(IChannel channel, IPlaylist[] playlists)
        {
            this._auddio = AudDClient
                .Create(
                    Env.GetString("audd_api_token")
                )
                .IncludeProvider(StreamProvider.Spotify)
                .IncludeProvider(StreamProvider.Apple_Music)
                .Build();


            this._channel = channel;
            this._playlists = playlists;

            // init pcmanager
            this._pcMgr = new PCManager(channel.Name);

            this._logPath = this._pcMgr.CombineChannel("log.txt");
            this._samplePath = this._pcMgr.CombineChannel("sample.mp3");

            // schedules
            Schedule(Execute).NonReentrant().ToRunEvery(30).Seconds();
        }

        public void Execute()
        {
            bool isSampleWritten = AsyncContext.Run(async () => await WriteChunkToFile(this._samplePath));

            if (!isSampleWritten)
            {
                // fail to write chunk to disk, so we exit and wait for next execution

                return;
            }

            var auddResp = this._auddio.RecognizeByFile(this._samplePath);
            var auddTrack = new AudDTrack(auddResp);

            if (!auddTrack.Success)
            {
                /*
                 if we got here, it means there wasn't any song playing because:
                 traffic highlights, breaking news, some talk show, or..
                 there's no recognition id for the current song (rare)
                */

                // prepare error text
                var error = new StringBuilder();
                error.AppendLine($"{DateTime.Now}");
                error.AppendLine(auddTrack.ToString());

                // print error to disk
                File.AppendAllText(this._logPath, error.ToString());

                return;
            }

            foreach (IPlaylist playlist in this._playlists)
            {
                AsyncContext.Run(async () =>
                {
                    await playlist.AddTrackToPlaylistAsync(auddTrack);
                });
            }

            var success = new StringBuilder();
            success.AppendLine(this._channel.Name);
            success.AppendLine(auddTrack.ToString());

            // print track to console
            Console.WriteLine(success.ToString());

            // print track to file
            File.AppendAllText(this._logPath, success.ToString());

            // Mechanism to wait the leftover in order to prevent
            // the detection of the same song in different offset
            // and a redundant call to the recognition api (reduce cost)
            var tsRemainder = this.GetNextTrackRemainder(auddTrack);

            // print remainder
            Console.WriteLine(tsRemainder.ToString("mm\\:ss"));

            // we call Duration() to convert the TimeSpan to absolute value
            TimeSpan tsWait = tsRemainder.Subtract(auddTrack.SegmentOffset).Duration();
            if (tsWait.Ticks > 0)
            {
                Console.WriteLine(tsWait.ToString("mm\\:ss"));
                Thread.Sleep(tsWait);
            }
        }

        private TimeSpan GetNextTrackRemainder(AudDTrack track)
        {
            return string.IsNullOrWhiteSpace(track.ProviderUri) ? 
                TimeSpan.FromMinutes(4) : 
                track.SegmentOffset;
        }

        // This method saves a 128KB chunk of the steam to a local file
        private async Task<bool> WriteChunkToFile(string fileName)
        {
            try
            {
                using var http = new HttpClient();
                using var stream = await http.GetStreamAsync(new Uri(_channel.StreamUrl));
                using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);

                byte[] buffer = Array.Empty<byte>();

                // 128KB is enough to sample, 0:08 seconds length
                await stream.ReadExactlyAsync(buffer, 0, 128000);
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