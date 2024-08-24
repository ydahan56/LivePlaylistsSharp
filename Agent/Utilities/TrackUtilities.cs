using Hanssens.Net;
using LivePlaylistsSharp.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Utilities
{
    public class TrackUtilities
    {
        private readonly ILocalStorage _localStorage;

        private const string LAST_TRACK_KEY = "last.track";

        public TrackUtilities(CommonUtilities commonUtilities)
        {
            this._localStorage = new LocalStorage(
                new LocalStorageConfiguration()
                {
                    Filename = commonUtilities.ChannelStoragePath
                }
            );
        }

        public void CommitTrackDisk(IPlaylistTrack track)
        {
            this._localStorage.Store(LAST_TRACK_KEY, track);
            this._localStorage.Persist();
        }

        public bool TrackExistInStorage(IPlaylistTrack track)
        {
            if (this._localStorage.Exists(LAST_TRACK_KEY))
            {
                var trackStorage = track.GetStorageTrack(
                    this._localStorage, 
                    LAST_TRACK_KEY
                );

                return track.Equals(trackStorage);
            }

            return false;
        }
    }
}
