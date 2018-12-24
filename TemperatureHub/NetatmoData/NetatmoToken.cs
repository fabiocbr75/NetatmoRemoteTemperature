using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureHub.NetatmoData
{
    public class NetatmoToken
    {
        public string Access_token { get; set; }

        public string Refresh_token { get; set; }

        public string[] Scope { get; set; }

        public long Expires_in { get; set; }

        public long Expire_in { get; set; }
    }
}
