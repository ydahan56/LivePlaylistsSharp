using Agent.Models;
using Hanssens.Net;
using LivePlaylistsSharp.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Utilities
{
    public class StorageUtilities
    {
        private readonly ILocalStorage _localStorage;

        private const string LAST_TRACK_KEY = "last.track";
        private const string LAST_STRGY_INDEX_KEY = "last.strgy.idx";
        private const string LAST_STRGY_RETRY_COUNTER = "last.strgy.retry.counter";

        public StorageUtilities(CommonUtilities commonUtilities)
        {
            this._localStorage = new LocalStorage(
                new LocalStorageConfiguration()
                {
                    Filename = commonUtilities.ChannelStoragePath
                }
            );
        }

        public void CommitStrategyRetryCount(int value)
        {
            this._localStorage.Store(LAST_STRGY_RETRY_COUNTER, value);
            this._localStorage.Persist();
        }

        public bool TryGetLastStrategyRetryCounter(out int value)
        {
            value = 0;

            if (this._localStorage.Exists(LAST_STRGY_RETRY_COUNTER))
            {
                value = this._localStorage.Get<int>(LAST_STRGY_RETRY_COUNTER);
                
                return true;
            }

            return false;
        }

        public void CommitCurrentStrategyIndex(int value)
        {
            this._localStorage.Store(LAST_STRGY_INDEX_KEY, value);
            this._localStorage.Persist();
        }

        public bool TryGetLastStrategyIndex(out int value)
        {
            // set default value
            value = 0;

            // do we have it stored?
            if (this._localStorage.Exists(LAST_STRGY_INDEX_KEY))
            {
                // read value
                value = this._localStorage.Get<int>(LAST_STRGY_INDEX_KEY);

                // return success
                return true;
            }

            return false;
        }

        public void CommitTrackDisk(IPlaylistTrack track)
        {
            this._localStorage.Store(LAST_TRACK_KEY, track);
            this._localStorage.Persist();
        }

        public bool StorageTrackEquals(IPlaylistTrack track)
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
