using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureHub.Models
{
    public class AggregateData : SensorData
    {
        public double TValve { get; set; }
        public double TCurrentTarget { get; set; }
        public double TCalculateTarget { get; set; }
        public double TScheduledTarget { get; set; }
        public bool SetTempSended { get; set; }
    }
}
