using Agent.Models;
using Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Extensions
{
    public static class ShazamExtensions
    {
        public static ShazamTrack ToTrack(this ShazamResult result)
        {
            return new ShazamTrack(result);
        }
    }
}
