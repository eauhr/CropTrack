namespace CropTrack.Models
{
    public class FieldCropModels
    {
        public int FieldId { get; set; }
        public int CropId { get; set; }
        public decimal QuantityInTons { get; set; }
        public DateTime PlantingDate { get; set; }
        public DateTime HarvestDate { get; set; }
    }
}
