using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
