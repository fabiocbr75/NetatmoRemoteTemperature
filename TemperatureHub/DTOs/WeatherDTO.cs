namespace TemperatureHub.DTOs
{
    public class WeatherDTO
    {
        public string SenderMAC { get; set; }
        public string SenderName { get; set; }
        public double Temperature { get; set; }
        public string Date { get; set; }
        public WeatherInfoDTO[] WeatherInfo { get; set; }
    }
}

