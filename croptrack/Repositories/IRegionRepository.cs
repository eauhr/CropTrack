using CropTrack.Models;

namespace CropTrack.Repositories
{
    public interface IRegionRepository
    {
        Task<List<Region>> GetAll();
        Task<Region> GetById(int id);
        Task Add(Region region);
        Task Update(Region region);
        Task Delete(Region region);
    }
}
