using CropTrack.Models;

namespace CropTrack.Services
{
    public interface IWeatherLogService
    {
        Task<List<WeatherLog>> GetWeatherLogsByRegion(int regionId);
        Task<WeatherLog> GetWeatherLogById(int id);
        Task AddWeatherLog(WeatherLog weatherLog);
        Task<bool> UpdateWeatherLog(WeatherLog weatherLog);
        Task<bool> DeleteWeatherLog(int id);
    }
}
