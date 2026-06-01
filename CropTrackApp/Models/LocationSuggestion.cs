using System;
using System.Collections.Generic;

namespace CropTrackApp.Models
{
    public class LocationSuggestion
    {
        public string Name { get; set; } = string.Empty;
        public string Admin1 { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string DisplayName
        {
            get
            {
                List<string> parts = new();
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    parts.Add(Name);
                }

                if (!string.IsNullOrWhiteSpace(Admin1) &&
                    !string.Equals(Admin1, Name, StringComparison.OrdinalIgnoreCase))
                {
                    parts.Add(Admin1);
                }

                if (!string.IsNullOrWhiteSpace(Country))
                {
                    parts.Add(Country);
                }

                return string.Join(", ", parts);
            }
        }
    }
}
