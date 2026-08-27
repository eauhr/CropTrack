using CropTrack.Models;

namespace CropTrack.Repositories
{
    public interface IFieldRepository
    {
        Task<List<Field>> GetAllByFarmerId(int farmerId);
        Task<Field> GetById(int id);
        Task Add(Field field);
        Task Update(Field field);
        Task Delete(Field field);
    }
}
