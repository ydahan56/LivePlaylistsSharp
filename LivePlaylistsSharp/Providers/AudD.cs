using DotNetEnv;
using LivePlaylistsClone.Contracts.Providers;
using LivePlaylistsClone.Models.AudD;
using RestSharp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LivePlaylistsClone.Providers
{
    public class AudD : IMusicProvider
    {
        private readonly string _api_key;
        private readonly RestClient _client;
        private readonly List<string> providers;

        private AudD()
        {
            this._api_key = Env.GetString("audd_api_key");
            this._client = new RestClient("https://api.audd.io/");
            this.providers = new List<string>();
        }

        public static AudD Create()
        {
            return new AudD();
        }

        public AudD IncludeAppleMusic()
        {
            this.providers.Add("apple_music");

            return this;
        }

        public AudD IncludeSpotify()
        {
            this.providers.Add("spotify");

            return this;
        }

        public async Task<IMusicResult> RecognizeSongByFile(string filePath)
        {
            var request = new RestRequest()
            {
                AlwaysMultipartFormData = true
            };

            request.AddFile(Path.GetFileName(filePath), filePath);
            request.AddParameter("api_token", this._api_key);
            request.AddParameter("return", string.Join(",", this.providers.ToArray()));

            var response = await this._client.PostAsync<Root>(request);

            return response;
        }

        public async Task<IMusicResult> RecognizeSongByUrl(string fileUrl)
        {
            var request = new RestRequest()
            {
                AlwaysMultipartFormData = true
            };

            request.AddParameter("url", fileUrl);
            request.AddParameter("api_token", this._api_key);
            request.AddParameter("return", string.Join(",", this.providers.ToArray()));

            var response = await this._client.PostAsync<Root>(request);

            return response;
        }
    }
}
