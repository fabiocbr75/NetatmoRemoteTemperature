namespace TemperatureHub.DTOs
{
    public class WeatherInfoDTO
    {
        public WeatherInfoDTO()
        {
            DateISO = string.Empty;
            DateOfWeek = string.Empty;
        }
        public string DateISO { get; set; }
        public string DateOfWeek { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
    }
}

