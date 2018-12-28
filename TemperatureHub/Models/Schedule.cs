using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureHub.Models
{
    public class Schedule
    {
        public int MinuteFromMonday { get; set; }
        public long EndTime { get; set; }
        public List<RoomSchedule> RoomSchedules { get; set; }
    }
}
