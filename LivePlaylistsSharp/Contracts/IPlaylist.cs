using LivePlaylistsSharp.Contracts;
using System.Threading.Tasks;

namespace LivePlaylistsClone.Contracts
{
    public interface IPlaylist
    {
        Task AddTrackToPlaylistAsync(IPlaylistTrack track);
    }
}
