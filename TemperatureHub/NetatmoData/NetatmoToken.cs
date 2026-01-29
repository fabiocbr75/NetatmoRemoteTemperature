using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TemperatureHub.NetatmoData
{
    public class NetatmoToken
    {
        [JsonPropertyName("access_token")]
        public string Access_token { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string Refresh_token { get; set; } = string.Empty;

        [JsonPropertyName("scope")]
        public string[] Scope { get; set; } = new string[0];

        [JsonPropertyName("expires_in")]
        public long Expires_in { get; set; } = 0;

        [JsonPropertyName("expire_in")]
        public long Expire_in { get; set; } = 0;
    }
}
