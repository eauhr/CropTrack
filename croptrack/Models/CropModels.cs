namespace CropTrack.Models
{
    public class CropModels
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public int AvgGrowthDays { get; set; }
        public decimal YieldPerAcre { get; set; }
        public decimal OptimalTemperature { get; set; }
    }
}
