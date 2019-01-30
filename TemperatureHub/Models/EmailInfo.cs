namespace TemperatureHub.Models
{
    public class EmailInfo
    {
        public string SmtpServer { get; set; }
        public string SmtpUserName { get; set; }
        public string SmtpPassword { get; set; }
        public string FromMailAddress { get; set; }
        public string ToMailAddress { get; set; }
    }
}