﻿using System.Collections.Generic;
using System.Threading.Tasks;
using TemperatureHub.Models;

namespace TemperatureHub.NetatmoData
{
    public interface INetatmoDataHandler
    {
        Task<Schedule> GetActiveRoomSchedule(string homeId, string accessToken);
        Task<List<RoomData>> GetRoomStatus(string homeId, string accessToken, long endSchedulateTime);
        Task<List<Schedule>> GetSchedule(string homeId, string accessToken);
        Task<NetatmoToken> GetToken();
        void Init(string clientId, string clientSecret, string refreshToken);

        //Task<NetatmoToken> GetTokenByRefresh(string clientId, string clientSecret, string refreshToken);
        Task<string> SetThemp(string homeId, string roomId, double temp, long endTime, string accessToken);
    }
}