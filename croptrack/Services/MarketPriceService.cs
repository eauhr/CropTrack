using CropTrack.Models;
using CropTrack.Repositories;

namespace CropTrack.Services
{
    public class MarketPriceService : IMarketPriceService
    {
        private readonly IMarketPriceRepository _marketPriceRepository;

        public MarketPriceService(IMarketPriceRepository marketPriceRepository)
        {
            _marketPriceRepository = marketPriceRepository;
        }

        public async Task<List<MarketPrice>> GetMarketPricesByCrop(int cropId)
        {
            return await _marketPriceRepository.GetAllByCropId(cropId);
        }

        public async Task<MarketPrice> GetMarketPriceById(int id)
        {
            return await _marketPriceRepository.GetById(id);
        }

        public async Task AddMarketPrice(MarketPrice marketPrice)
        {
            await _marketPriceRepository.Add(marketPrice);
        }

        public async Task<bool> UpdateMarketPrice(MarketPrice marketPrice)
        {
            MarketPrice existing = await _marketPriceRepository.GetById(marketPrice.MarketPriceId);
            if (existing == null)
                return false;

            existing.PricePerTon = marketPrice.PricePerTon;
            existing.DateRecorded = marketPrice.DateRecorded;

            await _marketPriceRepository.Update(existing);
            return true;
        }

        public async Task<bool> DeleteMarketPrice(int id)
        {
            MarketPrice marketPrice = await _marketPriceRepository.GetById(id);
            if (marketPrice == null)
                return false;

            await _marketPriceRepository.Delete(marketPrice);
            return true;
        }
    }
}
