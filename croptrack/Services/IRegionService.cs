using CropTrack.Models;

namespace CropTrack.Services
{
    public interface IRegionService
    {
        Task<List<Region>> GetAllRegions();
        Task<Region> GetRegionById(int id);
        Task AddRegion(Region region);
        Task UpdateRegion(Region region);
        Task<bool> DeleteRegion(int id);
    }
}
