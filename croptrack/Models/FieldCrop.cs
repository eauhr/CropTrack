namespace croptrack.Models
{
    public class FieldCrop
    {
        public int FieldCropId { get; set; }
        public int FieldId { get; set; }
        public int CropId { get; set; }
        public decimal QuantityInTons { get; set; }
        public DateTime PlantingDate { get; set; }
        public DateTime HarvestDate { get; set; }
        public Field Field { get; set; }
        public Crop Crop { get; set; }
    }
}
