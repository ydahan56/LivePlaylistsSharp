using LivePlaylistsClone.Contracts.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LivePlaylistsClone.Models.AudD.Apple
{
    public class AppleMusicRoot : IProviderUri
    {
        public List<Preview> previews { get; set; }
        public Artwork artwork { get; set; }
        public string artistName { get; set; }
        public string url { get; set; }
        public int discNumber { get; set; }
        public List<string> genreNames { get; set; }
        public int durationInMillis { get; set; }
        public string releaseDate { get; set; }
        public string name { get; set; }
        public string isrc { get; set; }
        public string albumName { get; set; }
        public PlayParams playParams { get; set; }
        public int trackNumber { get; set; }
        public string composerName { get; set; }


        public string Uri => this.url;
        public string ProviderName => "apple_music";
        public TimeSpan Duration => TimeSpan.FromMilliseconds(durationInMillis);
      
    }


}