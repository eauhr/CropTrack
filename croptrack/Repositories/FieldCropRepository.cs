using CropTrack.Data;
using CropTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CropTrack.Repositories
{
    public class FieldCropRepository : IFieldCropRepository
    {

        private readonly FieldDbTrackContext _context;

        public FieldCropRepository(FieldDbTrackContext context)
        {
            _context = context;
        }

        public async Task<List<FieldCrop>> GetAllByFieldId(int fieldId)
        {
            return await _context.FieldCrops
                .Include(fc => fc.Crop)
                .Where(fc => fc.FieldId == fieldId)
                .ToListAsync();
        }

        public async Task<FieldCrop> GetById(int id)
        {
            return await _context.FieldCrops
                .Include(fc => fc.Crop)
                .Include(fc => fc.Field)
                .FirstOrDefaultAsync(fc => fc.FieldCropId == id);
        }

        public async Task Add(FieldCrop fieldCrop)
        {
            _context.FieldCrops.Add(fieldCrop);
            await _context.SaveChangesAsync();
        }

        public async Task Update(FieldCrop fieldCrop)
        {
            _context.FieldCrops.Update(fieldCrop);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(FieldCrop fieldCrop)
        {
            _context.FieldCrops.Remove(fieldCrop);
            await _context.SaveChangesAsync();
        }
    }
}
