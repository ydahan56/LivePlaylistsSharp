using Hanssens.Net;
using System;
using System.Text;

namespace LivePlaylistsSharp.Contracts
{
    public abstract class IPlaylistTrack : IEquatable<IPlaylistTrack>
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string SpotifyUri { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan OffsetTime { get; set; }

        public bool Success { get; set; }
        public bool IdleEnabled { get; set; }
        public TimeSpan RetryTimeSpan { get; set; }


        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Title: {this.Title}");
            sb.AppendLine($"Artist: {this.Artist}");
            sb.AppendLine($"Uri: {this.SpotifyUri}");

            return sb.ToString();
        }

        public bool Equals(IPlaylistTrack other)
        {
            return this.ID.Equals(other.ID);
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

        public abstract IPlaylistTrack GetStorageTrack(
            ILocalStorage storage,
            string itemKey
        );
    }
}
