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

        public ShazamClient Build()
        {
            return this;
        }

        public ShazamResult RecognizeByFile(string filePath)
        {
            var request = new RestRequest(
                "/services/webrec/match_extension", 
                Method.Post
            );

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileDataBase64 = Convert.ToBase64String(fileBytes);

            var requestDTO = new RequestDto()
            {
                country = "US",
                data = fileDataBase64,
                lang = "en",
                sessionId = Guid.NewGuid().ToString().ToLower()
            };

            request.AddJsonBody<RequestDto>(requestDTO);

            try
            {
                return this._client.Post<ShazamResult>(request);
            }
            catch (Exception ex)
            {
                // print exception
                Console.WriteLine(ex.ToString());

                // return default
                return new ShazamResult();
            }
        }
    }
}
