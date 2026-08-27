using CropTrack.Models;

namespace CropTrack.Repositories
{
    public interface IRegionRepository
    {
        Task<List<Region>> GetAllByFarmerId(int farmerId);
        Task<Region?> GetByIdForFarmer(int id, int farmerId);
        Task Add(Region region);
        Task Update(Region region);
        Task Delete(Region region);
    }
}
