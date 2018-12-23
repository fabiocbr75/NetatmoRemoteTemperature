using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using TemperatureHub.Models;

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
        public async Task<NetatmoToken> GetTokenByRefresh(string clientId, string clientSecret, string refreshToken)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("grant_type", "password");
            dict.Add("client_id", clientId);
            dict.Add("client_secret", clientSecret);
            dict.Add("refresh_token", refreshToken);
            var client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.netatmo.com/oauth2/token") { Content = new FormUrlEncodedContent(dict) };
            var res = await client.SendAsync(req);
            var tokenJson = res.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<NetatmoToken>(tokenJson);
        }

        public async Task<List<RoomData>> GetHomeStatus(string homeId, string accessToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var builder = new UriBuilder("https://api.netatmo.com/api/homestatus");
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["home_id"] = homeId;
            builder.Query = query.ToString();
            string url = builder.ToString();
            List<RoomData> roomData = new List<RoomData>();
            try
            {
                var res = await client.GetStringAsync(url);
                var jobj = JObject.Parse(res);
                if (jobj["status"].ToString().Equals("ok", StringComparison.InvariantCultureIgnoreCase))
                {
                    roomData = jobj["body"]["home"]["rooms"].Select(x => new RoomData()
                    {
                        Id = x["id"].ToString(),
                        TCurrentTarget = x["therm_setpoint_temperature"].ToObject<double>(),
                        TValve = x["therm_measured_temperature"].ToObject<double>()
                    }).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return roomData;
        }

        public async Task<List<Schedule>> GetHomeData(string homeId, string accessToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var builder = new UriBuilder("https://api.netatmo.com/api/homesdata");
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["home_id"] = homeId;
            builder.Query = query.ToString();
            string url = builder.ToString();

            List<Schedule> roomSchedules = new List<Schedule>();
            try
            {
                var res = await client.GetStringAsync(url);
                var jobj = JObject.Parse(res);
                if (jobj["status"].ToString().Equals("ok", StringComparison.InvariantCultureIgnoreCase))
                {
                    var home = jobj["body"]["homes"].ToArray()[0];
                    var sched = home["schedules"].ToArray()[0];
                    roomSchedules = sched["timetable"].Select(x => new Schedule()
                    {
                        zoneId = x["zone_id"].ToString(),
                        MinuteFromMonday = x["m_offset"].ToObject<int>(),
                    }).ToList();

                    foreach (var item in roomSchedules)
                    {
                        var rooms = sched["zones"].Where(z => z["id"].ToString() == item.zoneId).Select(r => r["rooms"]).First();
                        item.RoomSchedules = rooms.Select(y => new RoomSchedule() { RoomId = y["id"].ToString(), TTarget = y["therm_setpoint_temperature"].ToObject<double>()}).ToArray();
                    }


                }
            }
            catch (Exception ex)
            {
                throw;
            }
                
            return roomSchedules;
        }

        public async Task<RoomSchedule[]> GetActiveRoomSchedule(string homeId, string accessToken)
        {
            var now = DateTime.Now;
            var dow = (int)(now.DayOfWeek + 6) % 7;
            var scheduleTime = (dow * 1440) + (now.Hour * 60) + (now.Minute);

            var homesData = await GetHomeData(homeId, accessToken);
            var room = homesData.Where(x => x.MinuteFromMonday < scheduleTime).OrderByDescending(y => y.MinuteFromMonday).First();
            return room.RoomSchedules;
        }
    }
}
