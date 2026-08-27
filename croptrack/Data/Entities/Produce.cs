namespace CropTrack.Models
{
    public class Produce
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        public int AvgDaysToHarvest { get; set; }
        public double PlantingDepthCm { get; set; }
        public double SpacingCm { get; set; }

        public double MinTempC { get; set; }
        public double MaxTempC { get; set; }
        public double IdealTempC { get; set; }
        public double MinPh { get; set; }
        public double MaxPh { get; set; }
        public WaterIntensity WaterIntensity { get; set; }
    }

    public enum WaterIntensity
    {
        Low = 0,
        Medium = 1,
        High = 2
    }
}
