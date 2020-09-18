namespace TemperatureHub.Models
{
	public class MinMaxData4Day
    {
        private double _min;
        private double _max;
        public string SenderMAC { get; set; }
        public string Day { get;  set; }
        public double MinT
        {
            get => _min;
            set => _min = System.Math.Round(value, 1);
        }
        public double MaxT
        {
            get => _max;
            set => _max = System.Math.Round(value, 1);
        }
        public string MinTime { get; set; }
        public string MaxTime { get; set; }
        public double Delta
        {
            get
            {
                return MaxT - MinT;
            }
        }
    }
}
