using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureHub.DTOs
{
    public class SensorMasterDataDTO
    {
        public string SenderMAC { get; set; }
        public string SenderName { get; set; }
        public string RoomId { get; set; }
        public bool NetatmoSetTemp { get; set; }
        public bool ExternalSensor { get; set; }
    }
}
