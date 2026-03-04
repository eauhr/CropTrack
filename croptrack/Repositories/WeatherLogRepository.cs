using CropTrack.Data;
using CropTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CropTrack.Repositories
{
    public class WeatherLogRepository : IWeatherLogRepository
    {
        private readonly FieldDbTrackContext _context;

    public WeatherLogRepository(FieldDbTrackContext context)
    {
        _context = context;
    }

    public async Task<List<WeatherLog>> GetAllByRegionId(int regionId)
    {
        return await _context.WeatherLogs
            .Where(wl => wl.RegionId == regionId)
            .OrderByDescending(wl => wl.DateRecorded)
            .ToListAsync();
    }

    public async Task<WeatherLog> GetById(int id)
    {
        return await _context.WeatherLogs.FindAsync(id);
    }

    public async Task Add(WeatherLog weatherLog)
    {
        _context.WeatherLogs.Add(weatherLog);
        await _context.SaveChangesAsync();
    }

    public async Task Update(WeatherLog weatherLog)
    {
        _context.WeatherLogs.Update(weatherLog);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(WeatherLog weatherLog)
    {
        _context.WeatherLogs.Remove(weatherLog);
        await _context.SaveChangesAsync();
    }
}
}
