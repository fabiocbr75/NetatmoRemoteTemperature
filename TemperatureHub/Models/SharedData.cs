using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureHub.Models
{
    public class SharedData : ISharedData
    {
        public SharedData()
        {
            LastSensorData = new Dictionary<string, (double Temperature, DateTime IngestionTime, double BatteryLevel, string SenderName, double ScheduledTemperature)>();
        }
        public IDictionary<string, (double Temperature, DateTime IngestionTime, double BatteryLevel, string SenderName, double ScheduledTemperature)> LastSensorData { get; private set; }
    }
}
