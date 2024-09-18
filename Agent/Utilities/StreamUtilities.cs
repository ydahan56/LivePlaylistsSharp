using Agent.Loggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Utilities
{
    public class StreamUtilities
    {
        private readonly CommonUtilities _commonUtilities;

        public StreamUtilities(CommonUtilities commonUtilities)
        {
            this._commonUtilities = commonUtilities;
        }

        public async Task<bool> WriteChunkToFileAsync(Uri uri, string fileName)
        {
            try
            {
                using var http = new HttpClient();
                using var stream = await http.GetStreamAsync(uri);
                using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);

                byte[] buffer = new byte[128000];

                // 128KB is enough to sample, 0:08 seconds length
                await stream.ReadExactlyAsync(buffer, 0, buffer.Length);
                await fileStream.WriteAsync(buffer);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog(ex.ToString());

                return false;
            }

            return true;
        }
    }
}
