namespace TemperatureHub.DTOs
{
	public class MinMaxData4DayDTO
    {
        public string MAC { get; set; }
        public string Day { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string MinTime { get; set; }
        public string MaxTime { get; set; }
    }
}

