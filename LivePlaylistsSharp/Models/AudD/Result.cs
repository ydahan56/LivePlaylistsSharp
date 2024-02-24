using System;
using System.Globalization;
using System.Text;
using LivePlaylistsClone.Models.AudD.Spotify;
using LivePlaylistsClone.Models.AudD.Apple;

namespace LivePlaylistsClone.Models.AudD
{
    public class Result
    {
        public string artist { get; set; }
        public string title { get; set; }
        public string album { get; set; }
        public string release_date { get; set; }
        public string label { get; set; }
        public string timecode { get; set; }
        public string song_link { get; set; }
        public AppleMusicRoot apple_music { get; set; }
        public SpotifyRoot spotify { get; set; }
    }


}