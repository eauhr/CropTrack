namespace CropTrack.Models
{
    public class MarketPriceModels
    {
        public int CropId { get; set; }
        public decimal PricePerTon { get; set; }
        public DateTime DateRecorded { get; set; }
    }
}
