using CropTrack.Models;
using CropTrack.Repositories;

namespace CropTrack.Services
{
    public class WeatherLogService : IWeatherLogService
    {
        private readonly IWeatherLogRepository _weatherLogRepository;

        public WeatherLogService(IWeatherLogRepository weatherLogRepository)
        {
            _weatherLogRepository = weatherLogRepository;
        }

        public async Task<List<WeatherLog>> GetWeatherLogsByRegion(int regionId)
        {
            return await _weatherLogRepository.GetAllByRegionId(regionId);
        }

        public async Task<WeatherLog> GetWeatherLogById(int id)
        {
            return await _weatherLogRepository.GetById(id);
        }

        public async Task AddWeatherLog(WeatherLog weatherLog)
        {
            await _weatherLogRepository.Add(weatherLog);
        }

        public async Task<bool> UpdateWeatherLog(WeatherLog weatherLog)
        {
            WeatherLog existing = await _weatherLogRepository.GetById(weatherLog.WeatherLogId);
            if (existing == null)
                return false;

            existing.Temperature = weatherLog.Temperature;
            existing.Rainfall = weatherLog.Rainfall;
            existing.Forecast = weatherLog.Forecast;
            existing.DateRecorded = weatherLog.DateRecorded;

            await _weatherLogRepository.Update(existing);
            return true;
        }

        public async Task<bool> DeleteWeatherLog(int id)
        {
            WeatherLog weatherLog = await _weatherLogRepository.GetById(id);
            if (weatherLog == null)
                return false;

            await _weatherLogRepository.Delete(weatherLog);
            return true;
        }
    }
}
