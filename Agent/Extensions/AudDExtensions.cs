using AudDSharp.Models;
using LivePlaylistsSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Extensions
{
    public static class AudDExtensions
    {
        public static AudDTrack ToTrack(this AudDResult result)
        {
            return AudDTrack.Create(result);
        }
    }
}
