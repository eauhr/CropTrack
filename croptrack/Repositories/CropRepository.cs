using CropTrack.Data;
using CropTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CropTrack.Repositories
{
    public class CropRepository : ICropRepository
    {
        private readonly FieldDbTrackContext _context;

        public CropRepository(FieldDbTrackContext context)
        {
            _context = context;
        }

        public async Task<List<Crop>> GetAllByFarmerId(int farmerId)
        {
            return await _context.Crops
                .Where(c => c.FarmerId == farmerId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Crop?> GetByIdForFarmer(int id, int farmerId)
        {
            return await _context.Crops
                .FirstOrDefaultAsync(c => c.CropId == id && c.FarmerId == farmerId);
        }

        public async Task Add(Crop crop)
        {
            _context.Crops.Add(crop);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Crop crop)
        {
            _context.Crops.Update(crop);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Crop crop)
        {
            _context.Crops.Remove(crop);
            await _context.SaveChangesAsync();
        }
    }
}
