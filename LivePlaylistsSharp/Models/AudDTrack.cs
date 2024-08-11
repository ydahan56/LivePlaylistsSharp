using AudDSharp.Models;
using LivePlaylistsSharp.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivePlaylistsSharp.Models
{
    public class AudDTrack : IPlaylistTrack
    {
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string ProviderUri { get; private set; }
        public TimeSpan SegmentOffset { get; private set; }

        public bool Success {  get; private set; }

        public AudDTrack(AudDResult result)
        {
            this.Title = result.result.title;
            this.Artist = result.result.artist;
            this.ProviderUri = result.result.spotify.uri;
            this.Success = result.status.Contains("success");
            this.SegmentOffset = TimeSpan.ParseExact(
                result.result.timecode,
                "mm\\:ss",
                CultureInfo.InvariantCulture
            );
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Title: {this.Title}");
            sb.AppendLine($"Artist: {this.Artist}");
            sb.AppendLine($"Uri: {this.ProviderUri}");

            return sb.ToString();
        }
    }
}
