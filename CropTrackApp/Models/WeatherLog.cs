using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CropTrackApp.Models
{
    public class WeatherLog
    {
        public int WeatherLogId { get; set; }
        public int RegionId { get; set; }
        public DateTime DateRecorded { get; set; }
        public decimal Temperature { get; set; }
        public decimal Rainfall { get; set; }
        public string Forecast { get; set; }
    
}
}
