using System;
using System.Collections.Generic;

namespace TemperatureHub.Models
{
    public interface ISharedData
    {
        IDictionary<string, (double Temperature, DateTime IngestionTime)> LastSensorData { get; }
    }
}