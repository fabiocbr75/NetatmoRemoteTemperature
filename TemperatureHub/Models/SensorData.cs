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

        public string senderMAC { get; set; }
        public double temperature
        {
            get => _temperature;
            set => _temperature = System.Math.Round(value, 1);
        }
        public double humidity
        {
            get => _humidity;
            set => _humidity = System.Math.Round(value, 1);
        }
        public string ingestionTimestamp { get; set; }
    }
}
