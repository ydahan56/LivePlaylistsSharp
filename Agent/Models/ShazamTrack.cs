using LivePlaylistsSharp.Contracts;
using ShazamSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class ShazamTrack : IPlaylistTrack
    {
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string ProviderUri { get; private set; }
        public TimeSpan TotalTime { get; private set; }
        public TimeSpan OffsetTime { get; private set; }

        public bool Success { get; private set; }

        public TimeSpan RetryTimeSpan { get; private set; }

        public ShazamTrack()
        {
            
        }

        public ShazamTrack(ShazamResult result)
        {
            // the offset check is to determine if we have a track matched
            // the match could be non-null, but who knows what to expect
            this.Success = result.matches != null;

            if (!Success)
            {
                // we're going to try again.. until no more needed
                this.RetryTimeSpan = TimeSpan.FromMilliseconds(result.retry);

                return;
            }

            // we only have a single match
            var match = result.matches.FirstOrDefault();

            this.Title = match.metadata.title;
            this.Artist = match.metadata.artist;
            // this.ProviderUri = match.key; 
            this.ProviderUri = "";

            this.TotalTime = this.GetTrackLength(result);
            this.OffsetTime = TimeSpan.FromMilliseconds(match.offset);
        }

        private TimeSpan GetTrackLength(ShazamResult result)
        {
            var match = Regex.Match(result.matches[0].weburl, "trackLength=(?<length>\\d)");

            if (match.Success)
            {
                var tracklen = Convert.ToInt32(match.Groups["trackLength"].Value);

                return TimeSpan.FromMilliseconds(tracklen);
            }

            return TimeSpan.Zero;
        }
    }
}
