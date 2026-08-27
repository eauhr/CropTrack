namespace CropTrack.Models
{
    public class Crop
    {
        public int CropId { get; set; }
        public int FarmerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int AvgGrowthDays { get; set; }
        public decimal YieldPerAcre { get; set; }
        public decimal OptimalTemperature { get; set; }
        public Farmer Farmer { get; set; } = null!;
        public ICollection<FieldCrop> FieldCrops { get; set; } = new List<FieldCrop>();
        public ICollection<MarketPrice> MarketPrices { get; set; } = new List<MarketPrice>();
    }
}
