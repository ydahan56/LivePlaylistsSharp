using AudDSharp.Models;
using Hanssens.Net;
using LivePlaylistsSharp.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LivePlaylistsSharp.Models
{
    public class AudDTrack : IPlaylistTrack
    {
        public AudDTrack() // default ctor for local storage
        {
            
        }

        public static AudDTrack Create(AudDResult result)
        {
            return new AudDTrack(result);
        }

        private AudDTrack(AudDResult result)
        {
            this.Success = result == null ? false : result.status.Contains("success");

            if (!this.Success)
            {
                // An error occured..

                return;
            }

            this.ID = result.result.spotify.id;
            this.Title = result.result.title;
            this.Artist = result.result.artist;
            this.SpotifyUri = result.result.spotify?.uri;

            this.TotalTime = TimeSpan
                .FromMilliseconds(
                    result.result.spotify.duration_ms
            );
           
            this.OffsetTime = TimeSpan.ParseExact(
                result.result.timecode,
                "mm\\:ss",
                CultureInfo.InvariantCulture
            );

            this.IdleEnabled = true;
        }

        public override IPlaylistTrack GetStorageTrack(
            ILocalStorage storage,
            string itemKey
            )
        {
            return storage.Get<AudDTrack>(itemKey);
        }
    }
}
