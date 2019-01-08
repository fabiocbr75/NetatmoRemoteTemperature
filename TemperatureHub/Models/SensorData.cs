using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureHub.Models
{
    public class SensorData
    {
        private double _temperature;
        private double _humidity;
        private double _tScheduledTarget;
        private double _tValve;

        public string SenderMAC { get; set; }
        public double Temperature
        {
            get => _temperature;
            set => _temperature = System.Math.Round(value, 1);
        }
        public double Humidity
        {
            get => _humidity;
            set => _humidity = System.Math.Round(value, 1);
        }
        public string IngestionTimestamp { get; set; }
        public double TValve
        {
            get => _tValve;
            set => _tValve = System.Math.Round(value, 1);
        }
        public double TScheduledTarget
        {
            get => _tScheduledTarget;
            set => _tScheduledTarget = System.Math.Round(value, 1);
        }

    }
}
