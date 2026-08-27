namespace CropTrack.Services
{
    public interface IWeatherService
    {
        Task<double?> GetCurrentTemperatureAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default);
    }
}
