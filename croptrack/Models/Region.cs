namespace croptrack.Models
{
    public class Region
    {
        public int RegionId { get; set; }
        public string Name { get; set; }
        public ICollection<Field> Fields { get; set; }
        public ICollection<WeatherLog> WeatherLogs { get; set; }
    }
}
