using CropTrack.Data;
using CropTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CropTrack.Repositories
{
    public class FarmerRepository
    {
        private readonly FieldDbTrackContext _context;

        public FarmerRepository(FieldDbTrackContext context)
        {
            _context = context;
        }

        public async Task<Farmer> GetByEmailAsync(string email)
        {
            return await _context.Farmers.FirstOrDefaultAsync(f => f.Email == email);
        }

        public async Task AddAsync(Farmer farmer)
        {
            _context.Farmers.Add(farmer);
            await _context.SaveChangesAsync();
        }
    }
}
