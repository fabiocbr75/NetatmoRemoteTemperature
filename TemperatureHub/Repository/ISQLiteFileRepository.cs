using System;
using System.Collections.Generic;
using TemperatureHub.Models;

namespace TemperatureHub.Repository
{
    public interface ISQLiteFileRepository 
    {
        void AddSensorData(SensorData sensorData);
        List<SensorData> LoadSensorData(string id, string from, string to);
        List<SensorDataEx> LoadSensorDataEx(string id, string from, string to);
        List<SensorMasterData> LoadSensorMasterData();
    }
}