namespace ShazamSharp.Models
{
    public class Match
    {
        public string key { get; set; }
        public string trackId { get; set; }
        public double offset { get; set; }
        public Metadata metadata { get; set; }
        public string type { get; set; }
        public string adamId { get; set; }
        public string weburl { get; set; }
    }


}
