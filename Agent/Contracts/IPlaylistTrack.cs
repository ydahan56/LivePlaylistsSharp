using System;

namespace LivePlaylistsSharp.Contracts
{
    public interface IPlaylistTrack
    {
        string Title { get; }
        string Artist { get; }
        string ProviderUri { get; }
        TimeSpan SegmentOffset { get; }
    }
}
