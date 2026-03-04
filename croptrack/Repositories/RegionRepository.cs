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

        public async Task<List<Region>> GetAll()
        {
            return await _context.Regions.ToListAsync();
        }

        public async Task<Region> GetById(int id)
        {
            return await _context.Regions.FindAsync(id);
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
