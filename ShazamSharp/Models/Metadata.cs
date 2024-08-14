namespace ShazamSharp.Models
{
    public class Metadata
    {
        public string title { get; set; }
        public List<Artist> artists { get; set; }
        public string isrc { get; set; }
        public string lyrics { get; set; }
        public string artist { get; set; }
        public string artistart { get; set; }
        public string artistarthq { get; set; }
        public string artistartls { get; set; }
        public Syncedlyrics syncedlyrics { get; set; }
        public Openin openin { get; set; }
        public string coverart { get; set; }
    }


}
