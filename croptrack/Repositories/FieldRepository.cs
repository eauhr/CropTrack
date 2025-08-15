using CropTrack.Data;
using CropTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CropTrack.Repositories
{
    public class FieldRepository : IDb<Field, int>
    {
        private readonly FieldDbTrackContext _context;

        public FieldRepository(FieldDbTrackContext context)
        {
            _context = context;
        }

        public void Create(Field item)
        {
            _context.Fields.Add(item);
            _context.SaveChanges();
        }

        public Field Read(int key, bool useNavigationalProperties = false, bool isReadOnly = false)
        {
            IQueryable<Field> query = _context.Fields.AsQueryable();

            if (isReadOnly)
                query = query.AsNoTracking();

            if (useNavigationalProperties)
            {
                query = query.Include(f => f.Farmer)
                           .Include(f => f.Region)
                           .Include(f => f.FieldCrops)
                           .ThenInclude(fc => fc.Crop);
            }

            return query.FirstOrDefault(f => f.FieldId == key);
        }

        public List<Field> ReadAll(bool useNavigationalProperties = false, bool isReadOnly = false)
        {
            IQueryable<Field> query = _context.Fields.AsQueryable();

            if (isReadOnly)
                query = query.AsNoTracking();

            if (useNavigationalProperties)
            {
                query = query.Include(f => f.Farmer)
                           .Include(f => f.Region)
                           .Include(f => f.FieldCrops)
                           .ThenInclude(fc => fc.Crop);
            }

            return query.ToList();
        }

        public void Update(Field item, bool useNavigationalProperties = false)
        {
            if (useNavigationalProperties)
            {
                _context.Update(item);
            }
            else
            {
                _context.Fields.Update(item);
            }
            _context.SaveChanges();
        }

        public void Delete(int key)
        {
            var field = Read(key);
            if (field != null)
            {
                _context.Fields.Remove(field);
                _context.SaveChanges();
            }
        }
    }
}
