using System;

namespace TemperatureHub.Repository
{
    public interface ISQLiteFileRepository
    {
        void AddSensorData(string senderMAC, double temperature, double humidity, DateTime ingestionTimestamp);
    }
}