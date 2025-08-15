using CropTrack.Data;
using CropTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CropTrack.Repositories
{
    public class CropRepository : IDb<Crop, int>
    {
        private readonly FieldDbTrackContext _context;

        public CropRepository(FieldDbTrackContext context)
        {
            _context = context;
        }

        public void Create(Crop item)
        {
            _context.Crops.Add(item);
            _context.SaveChanges();
        }

        public Crop Read(int key, bool useNavigationalProperties = false, bool isReadOnly = false)
        {
            IQueryable<Crop> query = _context.Crops.AsQueryable();

            if (isReadOnly)
                query = query.AsNoTracking();

            if (useNavigationalProperties)
            {
                query = query.Include(c => c.FieldCrops)
                           .ThenInclude(fc => fc.Field)
                           .Include(c => c.MarketPrices);
            }

            return query.FirstOrDefault(c => c.CropId == key);
        }

        public List<Crop> ReadAll(bool useNavigationalProperties = false, bool isReadOnly = false)
        {
            IQueryable<Crop> query = _context.Crops.AsQueryable();

            if (isReadOnly)
                query = query.AsNoTracking();

            if (useNavigationalProperties)
            {
                query = query.Include(c => c.FieldCrops)
                           .ThenInclude(fc => fc.Field)
                           .Include(c => c.MarketPrices);
            }

            return query.ToList();
        }

        public void Update(Crop item, bool useNavigationalProperties = false)
        {
            if (useNavigationalProperties)
            {
                _context.Update(item);
            }
            else
            {
                _context.Crops.Update(item);
            }
            _context.SaveChanges();
        }

        public void Delete(int key)
        {
            var crop = Read(key);
            if (crop != null)
            {
                _context.Crops.Remove(crop);
                _context.SaveChanges();
            }
        }
    }
}
