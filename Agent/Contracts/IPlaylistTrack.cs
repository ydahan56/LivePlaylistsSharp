using Hanssens.Net;
using System;
using System.Text;

namespace LivePlaylistsSharp.Contracts
{
    public abstract class IPlaylistTrack : IEquatable<IPlaylistTrack>
    {
        public string Title { get; protected set; }
        public string Artist { get; protected set; }
        public string SpotifyUri { get; protected set; }
        public TimeSpan TotalTime { get; protected set; }
        public TimeSpan OffsetTime { get; protected set; }

        public bool Success { get; protected set; }
        public bool SmartWaitEnabled { get; protected set; }
        public TimeSpan RetryTimeSpan { get; protected set; }


        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Title: {this.Title}");
            sb.AppendLine($"Artist: {this.Artist}");
            sb.AppendLine($"Uri: {this.SpotifyUri}");

            return sb.ToString();
        }

        public TimeSpan GetGapBetweenOffsetToEnd()
        {
            if (this.TotalTime == TimeSpan.Zero)
            {
                return TimeSpan
                    .FromMilliseconds(210000) // average song time, arbritrary
                    .Subtract(this.OffsetTime);
            }

            return this.TotalTime.Subtract(this.OffsetTime);
        }

        public bool Equals(IPlaylistTrack other)
        {
            return this.SpotifyUri.Equals(other.SpotifyUri);
        }

        public abstract IPlaylistTrack GetStorageTrack(
            ILocalStorage storage,
            string itemKey
        );
    }
}
