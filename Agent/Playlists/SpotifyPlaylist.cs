using DotNetEnv;
using LivePlaylistsClone.Contracts;
using LivePlaylistsSharp.Contracts;
using Nito.AsyncEx;
using SpotifyAPI.Web;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LivePlaylistsClone.Playlists
{
    public class SpotifyPlaylist : IPlaylist
    {
        private const int PLAYLIST_TRACKS_LIMIT = 300;
        private const int PLAYLIST_INSERT_OFFSET = 0;

        private readonly string _playlistId;
        private readonly SpotifyClient _spotifyAPI;

        public SpotifyPlaylist(string playlistId)
        {
            this._playlistId = playlistId;

            var config = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(
                    new AuthorizationCodeAuthenticator(
                        Env.GetString("spotify_client_id"),
                        Env.GetString("spotify_client_secret"),
                        new AuthorizationCodeTokenResponse()
                        {
                            RefreshToken = Env.GetString("refresh_token")
                        }
                    )
                    //new ClientCredentialsAuthenticator(
                    //    Env.GetString("spotify_client_id"),
                    //    Env.GetString("spotify_client_secret")
                    //)
                 );

            this._spotifyAPI = new SpotifyClient(config);
        }

        public Task AddTrackToPlaylistAsync(IPlaylistTrack track)
        {
            string spotifyUri = track.ProviderUri;

            if (string.IsNullOrWhiteSpace(spotifyUri))
            {
                var searchRequest = new SearchRequest(
                    SearchRequest.Types.Track, $"{track.Artist} {track.Title}"
                );

                var search = AsyncContext.Run(async () => await _spotifyAPI.Search.Item(searchRequest));
                var item = search.Tracks.Items.First();

                spotifyUri = item.Uri;
            }

            // read playlist
            var playlist = AsyncContext.Run(async () => await _spotifyAPI.Playlists.Get(_playlistId));

            // add the captured track to the top of the playlist
            AsyncContext.Run(async () => 
                await this.AddPlaylistItems(
                    this._playlistId, 
                    PLAYLIST_INSERT_OFFSET, 
                    spotifyUri
                )
            );

            // if the playlist reached the limit, then remove older tracks
            if (this.PlaylistReachedLimit(playlist.Tracks.Total.Value, out int[] indexes))
            {
                // remove the last track from the bottom of the playlist
                AsyncContext.Run(async () => 
                    await this.RemovePlaylistItems(
                        _playlistId,
                        playlist.SnapshotId,
                        [..indexes]
                    )
                );
            }

            // we're done
            return Task.CompletedTask;
        }

        private bool PlaylistReachedLimit(int count, out int[] itemsIndex)
        {
            itemsIndex = Array.Empty<int>();

            var ret = count >= PLAYLIST_TRACKS_LIMIT;

            if (ret)
            {
                var indexCount = count - PLAYLIST_TRACKS_LIMIT;
                itemsIndex = new int[indexCount];
                
                for ( var i = 0; i < indexCount; i++ )
                {
                    itemsIndex[i] = PLAYLIST_TRACKS_LIMIT + i;
                }
            }

            return ret;
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
