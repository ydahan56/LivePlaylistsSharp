using LivePlaylistsClone.Contracts.Providers;
using System;
using System.Collections.Generic;

namespace LivePlaylistsClone.Models.AudD.Spotify
{
    public class SpotifyRoot : IProviderUri
    {
        public Album album { get; set; }
        public List<Artist> artists { get; set; }
        public object available_markets { get; set; }
        public int disc_number { get; set; }
        public int duration_ms { get; set; }
        public bool @explicit { get; set; }
        public ExternalIds external_ids { get; set; }
        public ExternalUrls external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public bool is_local { get; set; }
        public string name { get; set; }
        public int popularity { get; set; }
        public int track_number { get; set; }
        public string type { get; set; }
        public string uri { get; set; }



        public string Uri => this.uri;
        public string ProviderName => "spotify";
        public TimeSpan Duration => TimeSpan.FromMilliseconds(duration_ms);
    }


}