namespace CropTrack.Models
{
    public class MarketPrice
    {
        public int MarketPriceId { get; set; }
        public int CropId { get; set; }
        public decimal PricePerTon { get; set; }
        public DateTime DateRecorded { get; set; }
        public Crop Crop { get; set; }
    }
}
