using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivePlaylistsClone.Contracts.Providers
{
    public interface IProviderUri
    {
        string Uri { get; }
        string ProviderName { get; }
        TimeSpan Duration { get; }
    }
}
