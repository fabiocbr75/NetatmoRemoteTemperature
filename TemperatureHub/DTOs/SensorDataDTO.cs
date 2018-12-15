namespace TemperatureHub.DTOs
{
    public class SensorDataDTO
    {
        public string MAC { get; set; }
        public double Temp { get; set; }
        public double Humidity { get; set; }
        public string IngestionTimestamp { get; set; }
    }
}
