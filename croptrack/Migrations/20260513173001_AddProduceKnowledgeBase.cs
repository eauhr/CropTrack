using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CropTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddProduceKnowledgeBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Produces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScientificName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvgDaysToHarvest = table.Column<int>(type: "int", nullable: false),
                    PlantingDepthCm = table.Column<double>(type: "float", nullable: false),
                    SpacingCm = table.Column<double>(type: "float", nullable: false),
                    MinTempC = table.Column<double>(type: "float", nullable: false),
                    MaxTempC = table.Column<double>(type: "float", nullable: false),
                    IdealTempC = table.Column<double>(type: "float", nullable: false),
                    MinPh = table.Column<double>(type: "float", nullable: false),
                    MaxPh = table.Column<double>(type: "float", nullable: false),
                    WaterIntensity = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produces", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Produces",
                columns: new[] { "Id", "AvgDaysToHarvest", "Category", "IdealTempC", "MaxPh", "MaxTempC", "MinPh", "MinTempC", "Name", "PlantingDepthCm", "ScientificName", "SpacingCm", "WaterIntensity" },
                values: new object[,]
                {
                    { 1, 75, "Fruit Vegetable", 24.0, 6.7999999999999998, 35.0, 6.0, 13.0, "Tomato", 0.59999999999999998, "Solanum lycopersicum", 50.0, "High" },
                    { 2, 95, "Vegetable", 18.0, 6.5, 30.0, 5.0, 8.0, "Potato", 10.0, "Solanum tuberosum", 30.0, "Medium" },
                    { 3, 75, "Vegetable", 18.0, 6.7999999999999998, 30.0, 6.0, 7.0, "Carrot", 1.0, "Daucus carota", 8.0, "Medium" },
                    { 4, 50, "Vegetable", 17.0, 7.0, 24.0, 6.0, 4.0, "Lettuce", 0.5, "Lactuca sativa", 25.0, "Medium" },
                    { 5, 45, "Vegetable", 16.0, 7.5, 24.0, 6.5, 4.0, "Spinach", 1.5, "Spinacia oleracea", 10.0, "Medium" },
                    { 6, 55, "Fruit Vegetable", 27.0, 7.0, 35.0, 6.0, 16.0, "Cucumber", 2.0, "Cucumis sativus", 45.0, "High" },
                    { 7, 80, "Fruit Vegetable", 24.0, 6.7999999999999998, 32.0, 6.0, 15.0, "Bell Pepper", 0.59999999999999998, "Capsicum annuum", 45.0, "Medium" },
                    { 8, 110, "Vegetable", 20.0, 6.7999999999999998, 30.0, 6.0, 7.0, "Onion", 1.5, "Allium cepa", 10.0, "Medium" },
                    { 9, 180, "Vegetable", 18.0, 7.0, 30.0, 6.0, 5.0, "Garlic", 4.0, "Allium sativum", 12.0, "Medium" },
                    { 10, 90, "Vegetable", 17.0, 7.5, 24.0, 6.0, 7.0, "Cabbage", 1.0, "Brassica oleracea var. capitata", 45.0, "Medium" },
                    { 11, 80, "Vegetable", 18.0, 7.0, 24.0, 6.0, 7.0, "Broccoli", 1.0, "Brassica oleracea var. italica", 45.0, "Medium" },
                    { 12, 85, "Vegetable", 18.0, 7.0, 24.0, 6.0, 7.0, "Cauliflower", 1.0, "Brassica oleracea var. botrytis", 50.0, "Medium" },
                    { 13, 85, "Fruit Vegetable", 27.0, 6.7999999999999998, 35.0, 5.5, 18.0, "Eggplant", 0.59999999999999998, "Solanum melongena", 60.0, "Medium" },
                    { 14, 55, "Vegetable", 22.0, 7.0, 30.0, 6.0, 12.0, "Green Bean", 2.5, "Phaseolus vulgaris", 12.0, "Medium" },
                    { 15, 65, "Vegetable", 16.0, 7.5, 24.0, 6.0, 5.0, "Pea", 3.0, "Pisum sativum", 7.0, "Medium" },
                    { 16, 105, "Grain", 24.0, 7.0, 35.0, 5.7999999999999998, 10.0, "Maize", 4.0, "Zea mays", 25.0, "Medium" },
                    { 17, 120, "Grain", 18.0, 7.5, 32.0, 6.0, 4.0, "Wheat", 3.0, "Triticum aestivum", 15.0, "Low" },
                    { 18, 120, "Grain", 27.0, 7.0, 35.0, 5.5, 16.0, "Rice", 2.0, "Oryza sativa", 20.0, "High" },
                    { 19, 105, "Grain Legume", 24.0, 7.0, 35.0, 6.0, 10.0, "Soybean", 3.5, "Glycine max", 20.0, "Medium" },
                    { 20, 100, "Oilseed", 23.0, 7.5, 35.0, 6.0, 10.0, "Sunflower", 2.5, "Helianthus annuus", 30.0, "Medium" },
                    { 21, 90, "Fruit", 20.0, 6.7999999999999998, 30.0, 5.5, 8.0, "Strawberry", 1.5, "Fragaria × ananassa", 35.0, "Medium" }
                });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Produces");
        }
    }
}
