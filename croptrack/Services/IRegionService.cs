using CropTrack.Models;

namespace CropTrack.Services
{
    public interface IRegionService
    {
        Task<List<Region>> GetAllRegions(int farmerId);
        Task<Region?> GetRegionById(int id, int farmerId);
        Task<int> AddRegion(Region region, int farmerId);
        Task<bool> UpdateRegion(Region region, int farmerId);
        Task<bool> DeleteRegion(int id, int farmerId);
    }
}
