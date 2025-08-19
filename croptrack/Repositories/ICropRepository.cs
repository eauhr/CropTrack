using CropTrack.Models;

namespace CropTrack.Repositories
{
    public interface ICropRepository
    {
        Task<IEnumerable<Crop>> GetAll();
        Task<Crop> GetById(int id);
        Task Add(Crop crop);
        Task Update(Crop crop);
        Task Delete(Crop crop);
    }
}
