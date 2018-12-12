using System;

namespace TemperatureHub.Repository
{
    public interface ISQLiteFileRepository
    {
        bool ContainsItem(string id);
        void AddSensorData(string senderMAC, double temperature, double humidity, DateTime ingestionTimestamp);
    }
}