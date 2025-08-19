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

        public async Task<IEnumerable<Crop>> GetAll()
        {
            return await _context.Crops.ToListAsync();
        }

        public async Task<Crop> GetById(int id)
        {
            return await _context.Crops.FindAsync(id);
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
