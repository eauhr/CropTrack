using CropTrack.Data;
using CropTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CropTrack.Repositories
{
    public class RegionRepository : IRegionRepository
    {
        private readonly FieldDbTrackContext _context;

        public RegionRepository(FieldDbTrackContext context)
        {
            _context = context;
        }

        public async Task<List<Region>> GetAllByFarmerId(int farmerId)
        {
            return await _context.Regions
                .Where(r => r.FarmerId == farmerId)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<Region?> GetByIdForFarmer(int id, int farmerId)
        {
            return await _context.Regions
                .FirstOrDefaultAsync(r => r.RegionId == id && r.FarmerId == farmerId);
        }

        public async Task Add(Region region)
        {
            _context.Regions.Add(region);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Region region)
        {
            _context.Regions.Update(region);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Region region)
        {
            _context.Regions.Remove(region);
            await _context.SaveChangesAsync();
        }
    }
}
