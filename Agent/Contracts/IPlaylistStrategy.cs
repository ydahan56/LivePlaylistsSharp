using LivePlaylistsSharp.Contracts;
using System.Threading.Tasks;

namespace LivePlaylistsClone.Contracts
{
    public interface IPlaylistStrategy
    {
        Task AddTrackToPlaylistAsync(IPlaylistTrack track);
    }
}
