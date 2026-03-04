using CropTrack.Models;

namespace CropTrack.Repositories
{
    public interface IWeatherLogRepository
    {
        Task<List<WeatherLog>> GetAllByRegionId(int regionId);
        Task<WeatherLog> GetById(int id);
        Task Add(WeatherLog weatherLog);
        Task Update(WeatherLog weatherLog);
        Task Delete(WeatherLog weatherLog);
    }
}
