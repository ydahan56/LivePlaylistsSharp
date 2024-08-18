using AudDSharp.Models;
using LivePlaylistsSharp.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LivePlaylistsSharp.Models
{
    public class AudDTrack : IPlaylistTrack, IEquatable<IPlaylistTrack>
    {
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string ProviderUri { get; private set; }
        public TimeSpan TotalTime { get; private set; }
        public TimeSpan OffsetTime { get; private set; }

        public bool Success { get; set; }

        public TimeSpan RetryTimeSpan { get; private set; }

        // default ctor for local storage
        public AudDTrack()
        {
            
        }

        public AudDTrack(AudDResult result)
        {
            this.Success = result.status.Contains("success");

            if (!this.Success)
            {
                // An error occured..

                return;
            }

            if (result.result == null)
            {
                // Broadcast, breaking news, traffic, etc...
                this.Success = false;

                return;
            }

            this.Title = result.result.title;
            this.Artist = result.result.artist;
            this.ProviderUri = result.result.spotify?.uri;

            this.TotalTime = TimeSpan
                .FromMilliseconds(
                    result.result.spotify.duration_ms
            );
           
            this.OffsetTime = TimeSpan.ParseExact(
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
        public bool Equals(IPlaylistTrack other)
        {
            return this.ProviderUri.Equals(other.ProviderUri);
        }
    }
}
