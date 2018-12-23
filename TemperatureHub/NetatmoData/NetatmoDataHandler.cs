using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TemperatureHub.NetatmoData
{
    public class NetatmoDataHandler
    {
        public async Task<NetatmoToken> GetToken(string clientId, string clientSecret, string userName, string password)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("grant_type", "password");
            dict.Add("client_id", clientId);
            dict.Add("client_secret", clientSecret);
            dict.Add("username", userName);
            dict.Add("password", password);
            dict.Add("scope", "read_thermostat write_thermostat");
            var client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.netatmo.com/oauth2/token") { Content = new FormUrlEncodedContent(dict) };
            var res = await client.SendAsync(req);
            var tokenJson = res.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<NetatmoToken>(tokenJson);
        }
    }
}
