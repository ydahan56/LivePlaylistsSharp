using System.Threading.Tasks;

namespace LivePlaylistsClone.Contracts.Providers
{
    public interface IMusicProvider
    {
        Task<IMusicResult> RecognizeSongByUrl(string fileUrl);
        Task<IMusicResult> RecognizeSongByFile(string filePath);
    }
}
