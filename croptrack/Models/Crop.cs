namespace croptrack.Models
{
    public class Crop
    {
        public int CropId { get; set; }
        public string Name { get; set; } 
        public string Unit { get; set; }
        public int AvgGrowthDays { get; set; }
        public decimal YieldPerAcre { get; set; }
        public decimal OptimalTemperature { get; set; }
        public ICollection<FieldCrop> FieldCrops { get; set; }
        public ICollection<MarketPrice> MarketPrices { get; set; }
    }
}
