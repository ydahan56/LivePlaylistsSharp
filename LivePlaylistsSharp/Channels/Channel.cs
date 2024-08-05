using FluentScheduler;
using LivePlaylistsClone.Common;
using LivePlaylistsClone.Contracts;
using LivePlaylistsClone.Contracts.Providers;
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
        private readonly IMusicProvider _provider;
        private readonly IChannel _channel;
        private readonly IPlaylist[] _playlists;

        private readonly PCManager _pcMgr;

        private readonly string _logPath;
        private readonly string _samplePath;


        public Channel(IChannel channel, IPlaylist[] playlists)
        {
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
            bool writeSuccess = AsyncContext.Run(async () => await WriteChunkToFile(this._samplePath));

            if (!writeSuccess)
            {
                // Failed to write chunk to file,
                // so we exit and wait for next execution
                return;
            }

            var sb = new StringBuilder();

            var apiResult = AsyncContext.Run(async () => await this._provider.RecognizeSongByFile(this._samplePath));

            if (!apiResult.IsSuccess)
            {
                /*
                 if we got here, it means there wasn't any song playing because:
                 traffic highlights, breaking news, or some talk show, or..
                 there's no recognition id for the current song (rare)
                */

                sb.AppendLine($"{DateTime.Now}");
                sb.AppendLine(apiResult.ToString());

                File.AppendAllText(this._logPath, sb.ToString());
                return;
            }

            foreach (IPlaylist playlist in this._playlists)
            {
                AsyncContext.Run(async () => await playlist.AddTrackToPlaylistAsync(apiResult));
            }

            sb = new StringBuilder();
            sb.AppendLine(this._channel.Name);
            sb.AppendLine(apiResult.ToString());

            Console.WriteLine(sb.ToString());
            File.AppendAllText(this._logPath, sb.ToString());

            // Mechanism to wait the leftover in order to prevent
            // the detection of the same song in different offset
            // and a redundant call to the recognition api (reduce cost)
            var timeLeftSpan =
                apiResult.Spotify is null ?
                TimeSpan.FromMinutes(4) :
                apiResult.SegmentOffset;

            Console.WriteLine(timeLeftSpan.ToString("mm\\:ss"));

            // we call Duration() to convert the TimeSpan to absolute value
            TimeSpan tsWait = timeLeftSpan.Subtract(apiResult.SegmentOffset).Duration();
            if (tsWait.Ticks > 0)
            {
                Console.WriteLine(tsWait.ToString("mm\\:ss"));
                Thread.Sleep(tsWait);
            }
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