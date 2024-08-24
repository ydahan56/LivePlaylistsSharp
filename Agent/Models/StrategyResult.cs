using LivePlaylistsSharp.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class StrategyResult
    {
        public IPlaylistTrack Track { get; set; }
        public Action<IPlaylistTrack> Action { get; set; }

        public StrategyResult(IPlaylistTrack track, Action<IPlaylistTrack> action)
        {
            this.Track = track;
            this.Action = action;
        }
    }
}
