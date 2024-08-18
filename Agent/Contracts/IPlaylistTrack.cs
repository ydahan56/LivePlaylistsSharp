using System;

namespace LivePlaylistsSharp.Contracts
{
    public interface IPlaylistTrack
    {
        string Title { get; }
        string Artist { get; }
        string ProviderUri { get; }
        TimeSpan TotalTime { get; }
        TimeSpan OffsetTime { get; }

        bool Success { get; }
        public TimeSpan RetryTimeSpan { get; }
    }
}
