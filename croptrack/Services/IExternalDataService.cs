namespace CropTrack.Services
{
    public interface IExternalDataService
    {
        Task<OpenMeteoWeatherResult?> FetchOpenMeteoWeatherAsync(string regionName, CancellationToken cancellationToken = default);
        Task<UsdaPriceResult?> FetchUsdaLatestPriceAsync(string cropName, CancellationToken cancellationToken = default);
    }

    public sealed class OpenMeteoWeatherResult
    {
        public DateTime RecordedAt { get; init; }
        public decimal TemperatureC { get; init; }
        public decimal RainfallMm { get; init; }
        public string Forecast { get; init; } = string.Empty;
        public decimal DailyMaxC { get; init; }
        public decimal DailyMinC { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
    }

    public sealed class UsdaPriceResult
    {
        public decimal Price { get; init; }
        public string Unit { get; init; } = string.Empty;
        public int Year { get; init; }
        public string Commodity { get; init; } = string.Empty;
        public string SourceNote { get; init; } = string.Empty;
    }
}
