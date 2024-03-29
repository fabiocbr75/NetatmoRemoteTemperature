﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using TemperatureHub.Helpers;
using TemperatureHub.Models;

namespace TemperatureHub.NetatmoData
{
    public class NetatmoDataHandler : INetatmoDataHandler
    {
        private readonly IMemoryCache _cache;
        private readonly ISharedData _sharedData = null;
        private readonly IOptions<AppSettings> _appSettings;
        private bool _isInitialized;
        private string _clientId;
        private string _clientSecret;
        private string _refresh_token;

        public NetatmoDataHandler(IMemoryCache cache, ISharedData sharedData, IOptions<AppSettings> appSettings)
        {
            _cache = cache;
            _sharedData = sharedData;
            _appSettings = appSettings;
        }

        public void Init(string clientId, string clientSecret, string refreshToken)
        {
            if (_isInitialized)
            {
                return;
            }
            _clientId = clientId;
            _clientSecret = clientSecret;

            var token = LoadToken(refreshToken);

            _refresh_token = token.Refresh_token;

            _isInitialized = true;
        }

        public async Task<NetatmoToken> GetToken()
        {
            Logger.Info("NetatmoDataHandler", "GetToken Get started");
            string key = $"Token4ClientId_{_clientId}";
            NetatmoToken token = null;
            if (_cache.TryGetValue<NetatmoToken>(key, out token))
            {
                Logger.Info("NetatmoDataHandler", "GetToken cached Get finished");
                return token;
            }

            var dict = new Dictionary<string, string>();
            dict.Add("grant_type", "refresh_token");
            dict.Add("client_id", _clientId);
            dict.Add("client_secret", _clientSecret);
            dict.Add("refresh_token", _refresh_token);
            dict.Add("scope", "read_thermostat write_thermostat");
            var client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.netatmo.com/oauth2/token") { Content = new FormUrlEncodedContent(dict) };
            var res = await client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var tokenJson = res.Content.ReadAsStringAsync().Result;
            token = JsonConvert.DeserializeObject<NetatmoToken>(tokenJson);

            _refresh_token = token.Refresh_token;

            _cache.Set<NetatmoToken>(key, token, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(token.Expire_in - 60)
                });
            _sharedData.CacheKey.Add(key);


            SaveToken(token);

            Logger.Info("NetatmoDataHandler", "GetToken Get finished");
            return token;
        }

        private void SaveToken(NetatmoToken netatmoToken)
        {
            try
            {
                var token = JsonConvert.SerializeObject(netatmoToken);
                System.IO.File.WriteAllText(Path.Combine(_appSettings.Value.DbFullPath, "token.json"), token);
            }
            catch (Exception ex)
            {
                Logger.Error("NetatmoDataHandler", $"Fail write token. Error:{ex.Message}"); 
            }
        }

        private NetatmoToken LoadToken(string refreshToken)
        {
            try
            {
                var token = System.IO.File.ReadAllText(Path.Combine(_appSettings.Value.DbFullPath, "token.json"));
                return JsonConvert.DeserializeObject<NetatmoToken>(token);
            }
            catch (Exception ex)
            {
                Logger.Error("NetatmoDataHandler", $"Fail read token. Error:{ex.Message}");
            }

            return new NetatmoToken()
            {
                Refresh_token = refreshToken
            };
        }




        public async Task<List<RoomData>> GetRoomStatus(string homeId, string accessToken, long endSchedulateTime)
        {
            Logger.Info("NetatmoDataHandler", "GetRoomStatus Get started");
            string key = $"RoomStatus4HomeId_{homeId}";
            List<RoomData> roomData = new List<RoomData>();
            if (_cache.TryGetValue<List<RoomData>>(key, out roomData))
            {
                Logger.Info("NetatmoDataHandler", "GetRoomStatus cached Get finished");
                return roomData;
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var builder = new UriBuilder("https://api.netatmo.com/api/homestatus");
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["home_id"] = homeId;
            builder.Query = query.ToString();
            string url = builder.ToString();
            string res = string.Empty;
            try
            {
                res = await client.GetStringAsync(url);
                var jobj = JObject.Parse(res);
                if (jobj["status"].ToString().Equals("ok", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (jobj["body"]["errors"] == null)
                    {
                        roomData = jobj["body"]["home"]["rooms"].Where(z => z["reachable"].ToObject<bool>() == true).Select(x => new RoomData()
                        {
                            RoomId = x["id"].ToString(),
                            TCurrentTarget = x["therm_setpoint_temperature"].ToObject<double>(),
                            TValve = x["therm_measured_temperature"].ToObject<double>(),
                            IsAway = (x["therm_setpoint_mode"].ToObject<string>().Equals("away", StringComparison.InvariantCultureIgnoreCase))
                        }).ToList();

                        if (!jobj["body"]["home"]["rooms"].Any(z => z["reachable"].ToObject<bool>() == false))
                        { //Cacheble only if all sensor are reachable
                            long simulateCacheTo = new DateTimeOffset(DateTime.UtcNow.AddMinutes(9)).ToUnixTimeSeconds();
                            long endCacheTime = simulateCacheTo > endSchedulateTime ? endSchedulateTime : simulateCacheTo;

                            _cache.Set<List<RoomData>>(key, roomData, new MemoryCacheEntryOptions
                            {
                                AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(endCacheTime)
                            });
                            _sharedData.CacheKey.Add(key);
                        }
                    }
                    else
                    {
                        //Logger.Warn("NetatmoDataHandler", $"GetRoomStatus Error on netatmo body: {res}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("NetatmoDataHandler", $"GetRoomStatus message: {res}, exception {ex.Message}");
                throw;
            }

            Logger.Info("NetatmoDataHandler", "GetRoomStatus Get finished");
            return roomData;
        }

        public async Task<string> SetThemp(string homeId, string roomId, double temp, long endTime, string accessToken)
        {
            Logger.Info("NetatmoDataHandler", "SetThemp Get started");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var builder = new UriBuilder("https://api.netatmo.com/api/setroomthermpoint");
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["home_id"] = homeId;
            query["room_id"] = roomId;
            query["mode"] = "manual";
            query["temp"] = temp.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            query["endtime"] = endTime.ToString();
            builder.Query = query.ToString();
            string url = builder.ToString();
            string result = url;
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Post, url);
                var res = await client.SendAsync(req);
                result += res.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                Logger.Error("NetatmoDataHandler", $"SetThemp exception {ex.Message}");
                throw;
            }
            Logger.Info("NetatmoDataHandler", "SetThemp Get finished");
            return result;
        }

        public async Task<List<Schedule>> GetSchedule(string homeId, string accessToken)
        {
            Logger.Info("NetatmoDataHandler", "GetSchedule Get started");
            string key = $"Schedule4HomeId_{homeId}";
            List<Schedule> roomSchedules = new List<Schedule>();
            if (_cache.TryGetValue<List<Schedule>>(key, out roomSchedules))
            {
                Logger.Info("NetatmoDataHandler", "GetSchedule cached Get finished");
                return roomSchedules;
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var builder = new UriBuilder("https://api.netatmo.com/api/homesdata");
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["home_id"] = homeId;
            builder.Query = query.ToString();
            string url = builder.ToString();

            string res = string.Empty;
            
            try
            {
                res = await client.GetStringAsync(url);
                var jobj = JObject.Parse(res);
                if (jobj["status"].ToString().Equals("ok", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (jobj["body"]["errors"] == null)
                    {
                        var home = jobj["body"]["homes"].ToArray()[0];
                        var sched = home["schedules"].ToArray().Where(x => x["selected"] != null && x["selected"].ToObject<bool>() == true).First();

                        roomSchedules = sched["timetable"]
                                            .Select(x => new Schedule()
                                            {
                                                MinuteFromMonday = x["m_offset"].ToObject<int>(),
                                                RoomSchedules = sched["zones"]
                                                                        .Where(z => z["id"].ToString() == x["zone_id"].ToString())
                                                                        .Select(r => r["rooms"]).First()
                                                                        .Select(y => new RoomSchedule()
                                                                        {
                                                                            RoomId = y["id"].ToString(),
                                                                            TScheduledTarget = y["therm_setpoint_temperature"].ToObject<double>()
                                                                        }).ToList()
                                            }).ToList();

                        _cache.Set<List<Schedule>>(key, roomSchedules, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
                        });
                        _sharedData.CacheKey.Add(key);

                    }
                    else
                    {
                        Logger.Warn("NetatmoDataHandler", $"GetSchedule Error on netatmo body: {res}");
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Error("NetatmoDataHandler", $"GetSchedule message: {res}, exception {ex.Message}");
                throw;
            }

            Logger.Info("NetatmoDataHandler", "GetSchedule Get finished");
            return roomSchedules;
        }

        public async Task<Schedule> GetActiveRoomSchedule(string homeId, string accessToken)
        {
            Logger.Info("NetatmoDataHandler", "GetActiveRoomSchedule Get started");
            var now = DateTime.Now;
            var dow = (int)(now.DayOfWeek + 6) % 7;
            var scheduleTime = (dow * 1440) + (now.Hour * 60) + (now.Minute);

            var homesData = await GetSchedule(homeId, accessToken);
            if (homesData == null)
            {
                return null;
            }
            var schedule = homesData.Where(x => x.MinuteFromMonday < scheduleTime).OrderByDescending(y => y.MinuteFromMonday).First();

            var nextSched = homesData.Where(x => x.MinuteFromMonday > scheduleTime).OrderBy(y => y.MinuteFromMonday).FirstOrDefault();
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
            Logger.Info("NetatmoDataHandler", "GetActiveRoomSchedule Get finished");
            return schedule;
        }


    }
}
