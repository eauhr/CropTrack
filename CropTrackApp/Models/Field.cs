using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CropTrackApp.Models
{
    public class Field
    {
        public int FieldId { get; set; }
        public int FarmerId { get; set; }
        public int RegionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal Acres { get; set; }
    }
}
