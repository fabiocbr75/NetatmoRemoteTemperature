using System;
using System.Collections.Generic;

namespace TemperatureHub.Models
{
    public interface ISharedData
    {
        IDictionary<string, (double Temperature, DateTime IngestionTime, double BatteryLevel, string SenderName)> LastSensorData { get; }
    }
}