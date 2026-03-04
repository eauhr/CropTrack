using CropTrack.Models;

namespace CropTrack.Services
{
    public interface IMarketPriceService
    {
        Task<List<MarketPrice>> GetMarketPricesByCrop(int cropId);
        Task<MarketPrice> GetMarketPriceById(int id);
        Task AddMarketPrice(MarketPrice marketPrice);
        Task<bool> UpdateMarketPrice(MarketPrice marketPrice);
        Task<bool> DeleteMarketPrice(int id);
    }
}
