using CropTrack.Models;

namespace CropTrack.Data
{
    public static class SeedData
    {
        public static List<Produce> Produces => new()
        {
            new Produce { Id = 1, Name = "Tomato", ScientificName = "Solanum lycopersicum", Category = "Fruit Vegetable", AvgDaysToHarvest = 75, PlantingDepthCm = 0.6, SpacingCm = 50, MinTempC = 13, MaxTempC = 35, IdealTempC = 24, MinPh = 6.0, MaxPh = 6.8, WaterIntensity = WaterIntensity.High },
            new Produce { Id = 2, Name = "Potato", ScientificName = "Solanum tuberosum", Category = "Vegetable", AvgDaysToHarvest = 95, PlantingDepthCm = 10, SpacingCm = 30, MinTempC = 8, MaxTempC = 30, IdealTempC = 18, MinPh = 5.0, MaxPh = 6.5, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 3, Name = "Carrot", ScientificName = "Daucus carota", Category = "Vegetable", AvgDaysToHarvest = 75, PlantingDepthCm = 1.0, SpacingCm = 8, MinTempC = 7, MaxTempC = 30, IdealTempC = 18, MinPh = 6.0, MaxPh = 6.8, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 4, Name = "Lettuce", ScientificName = "Lactuca sativa", Category = "Vegetable", AvgDaysToHarvest = 50, PlantingDepthCm = 0.5, SpacingCm = 25, MinTempC = 4, MaxTempC = 24, IdealTempC = 17, MinPh = 6.0, MaxPh = 7.0, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 5, Name = "Spinach", ScientificName = "Spinacia oleracea", Category = "Vegetable", AvgDaysToHarvest = 45, PlantingDepthCm = 1.5, SpacingCm = 10, MinTempC = 4, MaxTempC = 24, IdealTempC = 16, MinPh = 6.5, MaxPh = 7.5, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 6, Name = "Cucumber", ScientificName = "Cucumis sativus", Category = "Fruit Vegetable", AvgDaysToHarvest = 55, PlantingDepthCm = 2.0, SpacingCm = 45, MinTempC = 16, MaxTempC = 35, IdealTempC = 27, MinPh = 6.0, MaxPh = 7.0, WaterIntensity = WaterIntensity.High },
            new Produce { Id = 7, Name = "Bell Pepper", ScientificName = "Capsicum annuum", Category = "Fruit Vegetable", AvgDaysToHarvest = 80, PlantingDepthCm = 0.6, SpacingCm = 45, MinTempC = 15, MaxTempC = 32, IdealTempC = 24, MinPh = 6.0, MaxPh = 6.8, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 8, Name = "Onion", ScientificName = "Allium cepa", Category = "Vegetable", AvgDaysToHarvest = 110, PlantingDepthCm = 1.5, SpacingCm = 10, MinTempC = 7, MaxTempC = 30, IdealTempC = 20, MinPh = 6.0, MaxPh = 6.8, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 9, Name = "Garlic", ScientificName = "Allium sativum", Category = "Vegetable", AvgDaysToHarvest = 180, PlantingDepthCm = 4.0, SpacingCm = 12, MinTempC = 5, MaxTempC = 30, IdealTempC = 18, MinPh = 6.0, MaxPh = 7.0, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 10, Name = "Cabbage", ScientificName = "Brassica oleracea var. capitata", Category = "Vegetable", AvgDaysToHarvest = 90, PlantingDepthCm = 1.0, SpacingCm = 45, MinTempC = 7, MaxTempC = 24, IdealTempC = 17, MinPh = 6.0, MaxPh = 7.5, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 11, Name = "Broccoli", ScientificName = "Brassica oleracea var. italica", Category = "Vegetable", AvgDaysToHarvest = 80, PlantingDepthCm = 1.0, SpacingCm = 45, MinTempC = 7, MaxTempC = 24, IdealTempC = 18, MinPh = 6.0, MaxPh = 7.0, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 12, Name = "Cauliflower", ScientificName = "Brassica oleracea var. botrytis", Category = "Vegetable", AvgDaysToHarvest = 85, PlantingDepthCm = 1.0, SpacingCm = 50, MinTempC = 7, MaxTempC = 24, IdealTempC = 18, MinPh = 6.0, MaxPh = 7.0, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 13, Name = "Eggplant", ScientificName = "Solanum melongena", Category = "Fruit Vegetable", AvgDaysToHarvest = 85, PlantingDepthCm = 0.6, SpacingCm = 60, MinTempC = 18, MaxTempC = 35, IdealTempC = 27, MinPh = 5.5, MaxPh = 6.8, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 14, Name = "Green Bean", ScientificName = "Phaseolus vulgaris", Category = "Vegetable", AvgDaysToHarvest = 55, PlantingDepthCm = 2.5, SpacingCm = 12, MinTempC = 12, MaxTempC = 30, IdealTempC = 22, MinPh = 6.0, MaxPh = 7.0, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 15, Name = "Pea", ScientificName = "Pisum sativum", Category = "Vegetable", AvgDaysToHarvest = 65, PlantingDepthCm = 3.0, SpacingCm = 7, MinTempC = 5, MaxTempC = 24, IdealTempC = 16, MinPh = 6.0, MaxPh = 7.5, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 16, Name = "Maize", ScientificName = "Zea mays", Category = "Grain", AvgDaysToHarvest = 105, PlantingDepthCm = 4.0, SpacingCm = 25, MinTempC = 10, MaxTempC = 35, IdealTempC = 24, MinPh = 5.8, MaxPh = 7.0, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 17, Name = "Wheat", ScientificName = "Triticum aestivum", Category = "Grain", AvgDaysToHarvest = 120, PlantingDepthCm = 3.0, SpacingCm = 15, MinTempC = 4, MaxTempC = 32, IdealTempC = 18, MinPh = 6.0, MaxPh = 7.5, WaterIntensity = WaterIntensity.Low },
            new Produce { Id = 18, Name = "Rice", ScientificName = "Oryza sativa", Category = "Grain", AvgDaysToHarvest = 120, PlantingDepthCm = 2.0, SpacingCm = 20, MinTempC = 16, MaxTempC = 35, IdealTempC = 27, MinPh = 5.5, MaxPh = 7.0, WaterIntensity = WaterIntensity.High },
            new Produce { Id = 19, Name = "Soybean", ScientificName = "Glycine max", Category = "Grain Legume", AvgDaysToHarvest = 105, PlantingDepthCm = 3.5, SpacingCm = 20, MinTempC = 10, MaxTempC = 35, IdealTempC = 24, MinPh = 6.0, MaxPh = 7.0, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 20, Name = "Sunflower", ScientificName = "Helianthus annuus", Category = "Oilseed", AvgDaysToHarvest = 100, PlantingDepthCm = 2.5, SpacingCm = 30, MinTempC = 10, MaxTempC = 35, IdealTempC = 23, MinPh = 6.0, MaxPh = 7.5, WaterIntensity = WaterIntensity.Medium },
            new Produce { Id = 21, Name = "Strawberry", ScientificName = "Fragaria × ananassa", Category = "Fruit", AvgDaysToHarvest = 90, PlantingDepthCm = 1.5, SpacingCm = 35, MinTempC = 8, MaxTempC = 30, IdealTempC = 20, MinPh = 5.5, MaxPh = 6.8, WaterIntensity = WaterIntensity.Medium }
        };
    }
}
