using LivePlaylistsClone.Contracts.Providers;
using System.Threading.Tasks;

namespace LivePlaylistsClone.Contracts
{
    public interface IPlaylist
    {
        Task AddTrackToPlaylistAsync(IMusicResult result);
    }
}
