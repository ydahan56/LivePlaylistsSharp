using LivePlaylistsClone.Contracts.Providers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace LivePlaylistsClone.Models.AudD
{
    public class Root : IMusicResult
    {
        public string status { get; set; }
        public Result result { get; set; }


        public bool IsSuccess => this.status.Equals("success");

        public string Artist => this.result.artist;
        public string Title => this.result.title;
        public string Album => this.result.album;

        public DateTime ReleaseDate => DateTime.Parse(
            this.result.release_date
        );

        public string Label => this.result.label;
        public string TimeCode => this.result.timecode;
        public string SongLink => this.result.song_link;

        public TimeSpan SegmentOffset => TimeSpan.ParseExact(
            this.result.timecode, 
            "mm\\:ss", 
            CultureInfo.InvariantCulture
        );

        public IProviderUri Spotify => this.result.spotify;
        public IProviderUri AppleMusic => this.result.apple_music;
    }
}