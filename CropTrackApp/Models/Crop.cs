using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CropTrackApp.Models
{
    public class Crop
    {
        public int CropId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int AvgGrowthDays { get; set; }
        public decimal YieldPerAcre { get; set; }
        public decimal OptimalTemperature { get; set; }
    }
}
