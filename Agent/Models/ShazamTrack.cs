using Hanssens.Net;
using LivePlaylistsSharp.Contracts;
using Project;
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
        public ShazamTrack()
        {
            
        }

        public ShazamTrack(ShazamResult result)
        {
            this.Success = result == null ? false : result.Success;

            if (!Success)
            {
                this.RetryTimeSpan = result == null ? 
                    TimeSpan.Zero : 
                    TimeSpan.FromMilliseconds(result.RetryMs);

                return;
            }

            this.Title = result.Title;
            this.Artist = result.Artist;
            this.SpotifyUri = "";

            this.TotalTime = TimeSpan.FromSeconds(result.DurationSec);
            this.OffsetTime = TimeSpan.FromSeconds(result.MatchOffsetSec);

            this.SmartWaitEnabled = true;
        }

        public override IPlaylistTrack GetStorageTrack(
            ILocalStorage storage, 
            string itemKey
            )
        {
            return storage.Get<ShazamTrack>(itemKey);
        }
    }
}
