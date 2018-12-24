using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using TemperatureHub.Helpers;
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

        public async Task<List<RoomData>> GetRoomStatus(string homeId, string accessToken)
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
                        RoomId = x["id"].ToString(),
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

        public async Task<List<Schedule>> GetSchedule(string homeId, string accessToken)
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
                    var sched = home["schedules"].ToArray().Where(x => x["selected"] != null).First();

                    roomSchedules = sched["timetable"]
                                        .Select(x => new Schedule()
                                            {
                                                MinuteFromMonday = x["m_offset"].ToObject<int>(),
                                                RoomSchedules = sched["zones"]
                                                                    .Where(z => z["id"].ToString() == x["zone_id"].ToString())
                                                                    .Select(r => r["rooms"]).First()
                                                                    .Select(y => new RoomSchedule() { RoomId = y["id"].ToString(),
                                                                                                      TScheduleTarget = y["therm_setpoint_temperature"].ToObject<double>()
                                                                                                    }).ToList()
                                            }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
                
            return roomSchedules;
        }

        public async Task<Schedule> GetActiveRoomSchedule(string homeId, string accessToken)
        {
            var now = DateTime.Now;
            var dow = (int)(now.DayOfWeek + 6) % 7;
            var scheduleTime = (dow * 1440) + (now.Hour * 60) + (now.Minute);

            var homesData = await GetSchedule(homeId, accessToken);
            var schedule = homesData.Where(x => x.MinuteFromMonday < scheduleTime).OrderByDescending(y => y.MinuteFromMonday).First();

            var nextSched = homesData.Where(x => x.MinuteFromMonday > scheduleTime).OrderByDescending(y => y.MinuteFromMonday).FirstOrDefault();
            if (nextSched != null)
            {
                int minuteToNextSched = nextSched.MinuteFromMonday - scheduleTime;
                DateTimeOffset dto = new DateTimeOffset(now.AddMinutes(Convert.ToDouble(minuteToNextSched)));
                schedule.EndTime = dto.ToUnixTimeSeconds();
            }
            else
            {
                var nextSunday = DateTimeHelper.Next(now, DayOfWeek.Sunday);
                DateTimeOffset dto = new DateTimeOffset(new DateTime(nextSunday.Year, nextSunday.Month, nextSunday.Day, 23, 59, 59));
                schedule.EndTime = dto.ToUnixTimeSeconds();
            }

            return schedule;
        }
    }
}
