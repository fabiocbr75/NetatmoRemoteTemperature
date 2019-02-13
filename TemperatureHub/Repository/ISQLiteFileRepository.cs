using System;
using System.Collections.Generic;
using TemperatureHub.Models;

namespace TemperatureHub.Repository
{
    public interface ISQLiteFileRepository
    {
        void AddAggregateData(AggregateData aggregateData);
        List<SensorData> LoadSensorData(string id, string from, string to);
        List<AggregateDataEx> LoadSensorDataEx(string id, string from, string to);
        List<SensorMasterData> LoadSensorMasterData();
        List<EmailInfo> LoadEmailInfo();
        List<WeatherInfo> LoadWeatherInfo(string mac, int lastDays);
        SensorMasterData SwitchPower(string id, bool power);
    }
}