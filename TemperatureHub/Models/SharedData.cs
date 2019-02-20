using System;
using System.Collections.Concurrent;
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
            LogQueue = new ConcurrentQueue<(int ErrorType, string Context, string Message)>();
            CacheKey = new HashSet<string>();
        }
        public IDictionary<string, (double Temperature, DateTime IngestionTime, double BatteryLevel, string SenderName, double ScheduledTemperature)> LastSensorData { get; private set; }

        public ConcurrentQueue<(int ErrorType, string Context, string Message)> LogQueue { get; private set; }

        public HashSet<string> CacheKey { get; private set; }

    }
}
