namespace CropTrack.Models
{
    public class Region
    {
        public int RegionId { get; set; }
        public int FarmerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Farmer Farmer { get; set; } = null!;
        public ICollection<Field> Fields { get; set; } = new List<Field>();
        public ICollection<WeatherLog> WeatherLogs { get; set; } = new List<WeatherLog>();
    }
}
