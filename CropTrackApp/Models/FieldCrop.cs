using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CropTrackApp.Models
{
    public class FieldCrop
    {
        public int FieldCropId { get; set; }
        public int FieldId { get; set; }
        public int CropId { get; set; }
        public string CropName { get; set; }
        public decimal QuantityInTons { get; set; }
        public DateTime PlantingDate { get; set; }
        public DateTime HarvestDate { get; set; }
    }
}
