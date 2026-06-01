using CropTrack.Models;

namespace CropTrack.Repositories
{
    public interface ICropRepository
    {
        Task<List<Crop>> GetAllByFarmerId(int farmerId);
        Task<Crop?> GetByIdForFarmer(int id, int farmerId);
        Task Add(Crop crop);
        Task Update(Crop crop);
        Task Delete(Crop crop);
    }
}
