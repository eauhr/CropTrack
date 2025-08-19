using CropTrack.Data;
using CropTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CropTrack.Repositories
{
    public class FieldRepository : IFieldRepository
    {
        private readonly FieldDbTrackContext _context;

        public FieldRepository(FieldDbTrackContext context)
        {
            _context = context;
        }

        public async Task<List<Field>> GetAllByFarmerId(int farmerId)
        {
            return await _context.Fields.Where(f => f.FarmerId == farmerId).ToListAsync();
        }

        public async Task<Field> GetById(int id)
        {
            return await _context.Fields
                .Include(f => f.FieldCrops)
                .FirstOrDefaultAsync(f => f.FieldId == id);
        }

        public async Task Add(Field field)
        {
            _context.Fields.Add(field);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Field field)
        {
            _context.Fields.Update(field);
            await _context.SaveChangesAsync();
        }


        public async Task Delete(Field field)
        {
            _context.Fields.Remove(field);
            await _context.SaveChangesAsync();
        }
    }
}
