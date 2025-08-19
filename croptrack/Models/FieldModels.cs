namespace CropTrack.Models
{
    public class FieldModels
    {
        public int FieldId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public decimal Acres { get; set; }
        public int RegionId { get; set; }
    }
}
