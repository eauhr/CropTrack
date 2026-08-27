using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CropTrackApp.Models
{
    public class MarketPrice
    {
        public int MarketPriceId { get; set; }
        public int CropId { get; set; }
        public decimal PricePerTon { get; set; }
        public DateTime DateRecorded { get; set; }
    }
}
