using CropTrack.Data;
using CropTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CropTrack.Repositories
{
    public class MarketPriceRepository : IMarketPriceRepository
    {
        private readonly FieldDbTrackContext _context;

        public MarketPriceRepository(FieldDbTrackContext context)
        {
            _context = context;
        }

        public async Task<List<MarketPrice>> GetAllByCropId(int cropId)
        {
            return await _context.MarketPrices
                .Where(mp => mp.CropId == cropId)
                .OrderByDescending(mp => mp.DateRecorded)
                .ToListAsync();
        }

        public async Task<MarketPrice> GetById(int id)
        {
            return await _context.MarketPrices.FindAsync(id);
        }

        public async Task Add(MarketPrice marketPrice)
        {
            _context.MarketPrices.Add(marketPrice);
            await _context.SaveChangesAsync();
        }

        public async Task Update(MarketPrice marketPrice)
        {
            _context.MarketPrices.Update(marketPrice);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(MarketPrice marketPrice)
        {
            _context.MarketPrices.Remove(marketPrice);
            await _context.SaveChangesAsync();
        }
    }
}
