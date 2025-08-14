namespace croptrack.Models
{
    public class WeatherLog
    {
        public int WeatherLogId { get; set; }
        public int RegionId { get; set; }
        public DateTime DateRecorded { get; set; }
        public decimal Temperature { get; set; }
        public decimal Rainfall { get; set; }
        public string Forecast { get; set; }
        public Region Region { get; set; }
    }
}
