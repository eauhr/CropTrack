using CropTrack.Data;
using CropTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CropTrack.Repositories
{
    public class FarmerRepository : IFarmerRepository
    {
        private readonly FieldDbTrackContext _context;

        public FarmerRepository(FieldDbTrackContext context)
        {
            _context = context;
        }

        public async Task<Farmer> GetByEmail(string email)
        {
            return await _context.Farmers.FirstOrDefaultAsync(f => f.Email == email);
        }

        public async Task Add(Farmer farmer)
        {
            _context.Farmers.Add(farmer);
            await _context.SaveChangesAsync();
        }
    }
}
