using Agent.Models;
using LivePlaylistsSharp.Contracts;
using Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Contracts
{
    public abstract class ITrackStrategy
    {
        public string Name { get; protected set; }

        // contains the total amount of retries
        protected int RetryCount;

        // keeps track of retries left
        protected int RetryCounter;

        // when this returns false, we move to the next strategy in the iterator
        public bool HasRetries()
        {
            var result = this.RetryCounter-- > 0;

            if (!result)
            {
                this.ResetCounter();
            }

            return result;
        }

        public void ResetCounter()
        {
            this.RetryCounter = this.RetryCount;
        }

        public void SetCounter(int value)
        {
            this.RetryCounter = value;
        }

        public abstract Task<StrategyResult> RunAsync(
            string filePath,
            Action<IPlaylistTrack> successCallback,
            Action<IPlaylistTrack> errorCallback
        );
    }
}
