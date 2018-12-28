using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureHub.Models
{
    public class RoomSchedule
    {
        public string RoomId { get; set; }
        public double TScheduledTarget { get; set; }
    }
}
