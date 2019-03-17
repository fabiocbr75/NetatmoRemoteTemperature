namespace TemperatureHub.Models
{
    public class SensorMasterData
    {
        [SQLite.PrimaryKey]
        public string SenderMAC { get; set; }
        public string SenderName { get; set; }
        public string RoomId { get; set; }
        public bool Enabled { get; set; }
        public bool ExternalSensor { get; set; }
        public bool NetatmoLinkEnabled { get; set; }

    }
}