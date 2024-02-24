using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivePlaylistsClone.Contracts.Providers
{
    public interface IMusicResult
    {
        bool IsSuccess { get; }

        string Artist { get; }
        string Title { get; }
        string Album { get; }
        DateTime ReleaseDate { get; }
        string Label { get; }
        string TimeCode { get; }
        string SongLink { get; }
        TimeSpan SegmentOffset { get; }

        IProviderUri Spotify {  get; }
        IProviderUri AppleMusic { get; }

        string ToString();
    }
}
