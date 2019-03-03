using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureHub.Models
{
    public class RoomData
    {
        public string RoomId { get; set; }
        public double TValve { get; set; }
        public double TCurrentTarget { get; set; }
        public bool IsAway { get; set; }
    }
}
