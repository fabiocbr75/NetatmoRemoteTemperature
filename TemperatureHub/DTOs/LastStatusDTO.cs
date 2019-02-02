namespace TemperatureHub.DTOs
{
    public class LastStatusDTO
    {
        public string MAC { get; set; }
        public double Temp { get; set; }
        public double BatteryLevel { get; set; }
        public string IngestionTimestamp { get; set; }
        public string SenderName { get; set; }
        public double ScheduledTemperature { get; set; }
    }
}
