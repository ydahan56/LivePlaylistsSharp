using DotNetEnv;
using LivePlaylistsClone.Contracts;
using LivePlaylistsClone.Contracts.Providers;
using SpotifyAPI.Web;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LivePlaylistsClone.Playlists
{
    public class SpotifyPlaylist : IPlaylist
    {
        private const int PLAYLIST_LIMIT = 100;

        private readonly string _playlistId;
        private readonly SpotifyClient _spotifyAPI;

        public SpotifyPlaylist(string playlistId)
        {
            this._playlistId = playlistId;

            var config = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(
                    new ClientCredentialsAuthenticator(
                        Env.GetString("spotify_client_id"),
                        Env.GetString("spotify_client_secret")
                    )
                 );

            this._spotifyAPI = new SpotifyClient(config);
        }

        public async Task AddTrackToPlaylistAsync(IMusicResult result)
        {
            try
            {
                await this.DoAddTrackToPlaylistAsync(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task DoAddTrackToPlaylistAsync(IMusicResult result)
        {
            var uri = result.Spotify.Uri;

            if (string.IsNullOrWhiteSpace(uri))
            {
                var searchRequest = new SearchRequest(
                    SearchRequest.Types.Track, $"{result.Artist} {result.Title}"
                );

                var search = await _spotifyAPI.Search.Item(searchRequest);
                var item = search.Tracks.Items.First();

                uri = item.Uri;
            }

            // read playlist
            var playlist = await _spotifyAPI.Playlists.Get(_playlistId);

            // if the playlist contains 100 items or above
            if (this.PlaylistReachedLimit(playlist.Tracks.Items.Count))
            {
                // remove the last track from the bottom of the playlist
                await this.RemovePlaylistItems(_playlistId, playlist.SnapshotId, 99);
            }

            // add the captured track to the top of the playlist
            await this.AddPlaylistItems(this._playlistId, 0, uri);
        }

        private bool PlaylistReachedLimit(int count)
        {
            return count >= PLAYLIST_LIMIT;
        }

        private async Task RemovePlaylistItems(
            string playlistId,
            string snapshotId,
            params int[] itemsIndex)
        {
            var request = new PlaylistRemoveItemsRequest()
            {
                Positions = itemsIndex,
                SnapshotId = snapshotId
            };

            await _spotifyAPI.Playlists.RemoveItems(playlistId, request);
        }

        private async Task AddPlaylistItems(
            string playlistId,
            int insertIndex,
            params string[] items)
        {
            var request = new PlaylistAddItemsRequest(items)
            {
                Position = insertIndex
            };

            await _spotifyAPI.Playlists.AddItems(playlistId, request);
        }
    }
}
