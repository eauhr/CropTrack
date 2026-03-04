using CropTrack.Models;

namespace CropTrack.Repositories
{
    public interface IMarketPriceRepository
    {
        Task<List<MarketPrice>> GetAllByCropId(int cropId);
        Task<MarketPrice> GetById(int id);
        Task Add(MarketPrice marketPrice);
        Task Update(MarketPrice marketPrice);
        Task Delete(MarketPrice marketPrice);
    }
}
