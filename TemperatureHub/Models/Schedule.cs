using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureHub.Models
{
    public class Schedule
    {
        public string zoneId { get; set; } 
        public int MinuteFromMonday { get; set; }
        public RoomSchedule[] RoomSchedules { get; set; }
    }
}
