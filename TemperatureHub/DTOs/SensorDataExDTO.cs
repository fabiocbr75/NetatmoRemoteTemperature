namespace TemperatureHub.DTOs
{
    public class SensorDataExDTO
    {
        public string MAC { get; set; }
        public string Name { get; set; }
        public double Temp { get; set; }
        public double HeatIndex { get; set; }
        public double Humidity { get; set; }
        public string IngestionTimestamp { get; set; }
        public double TValve { get; set; }
        public double TScheduledTarget { get; set; }

    }
}
