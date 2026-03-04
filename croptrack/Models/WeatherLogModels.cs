namespace CropTrack.Models
{
    public class WeatherLogModels
    {   
        public int RegionId { get; set; }
        public DateTime DateRecorded { get; set; }
        public decimal Temperature { get; set; }
        public decimal Rainfall { get; set; }
        public string Forecast { get; set; }
    }
}
