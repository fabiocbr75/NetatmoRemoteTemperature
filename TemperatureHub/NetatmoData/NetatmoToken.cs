using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureHub.NetatmoData
{
    public class NetatmoToken
    {
        public string Access_token { get; set; } = string.Empty;

        public string Refresh_token { get; set; } = string.Empty;

        public string[] Scope { get; set; } = new string[0];

        public long Expires_in { get; set; } = 0;

        public long Expire_in { get; set; } = 0;
    }
}
