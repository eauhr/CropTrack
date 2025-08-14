namespace croptrack.Models
{
    public class Field
    {
        public int FieldId { get; set; }
        public int FarmerId { get; set; }
        public int RegionId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public decimal Acres { get; set; }
        public Farmer Farmer { get; set; }
        public Region Region { get; set; }
        public ICollection<FieldCrop> FieldCrops { get; set; }
    }
}
