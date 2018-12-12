using System;
using TemperatureHub.Models;

namespace TemperatureHub.Repository
{
    public interface ISQLiteFileRepository
    {
        void AddSensorData(SensorData sensorData);
    }
}