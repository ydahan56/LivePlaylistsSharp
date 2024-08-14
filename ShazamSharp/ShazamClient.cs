using RestSharp;
using ShazamSharp.Models;

namespace ShazamSharp
{
    public class ShazamClient
    {
        private readonly RestClient _client;

        private ShazamClient()
        {
            this._client = new RestClient("https://www.shazam.com");
        }

        public static ShazamClient Create()
        {
            return new ShazamClient();
        }

        public ShazamResult RecognizeByFile(string filePath)
        {
            var request = new RestRequest(
                "/services/webrec/match_extension", 
                Method.Post
            );


        }
    }
}
