using CropTrack.Models;
using CropTrack.Repositories;

namespace CropTrack.Services
{
    public class RegionService : IRegionService
    {
        private readonly IRegionRepository _regionRepository;

        public RegionService(IRegionRepository regionRepository)
        {
            _regionRepository = regionRepository;
        }

        public async Task<List<Region>> GetAllRegions(int farmerId)
        {
            return await _regionRepository.GetAllByFarmerId(farmerId);
        }

        public async Task<Region?> GetRegionById(int id, int farmerId)
        {
            return await _regionRepository.GetByIdForFarmer(id, farmerId);
        }

        public async Task<int> AddRegion(Region region, int farmerId)
        {
            region.FarmerId = farmerId;
            await _regionRepository.Add(region);
            return region.RegionId;
        }

        public async Task<bool> UpdateRegion(Region region, int farmerId)
        {
            Region? existing = await _regionRepository.GetByIdForFarmer(region.RegionId, farmerId);
            if (existing == null)
            {
                return false;
            }

            existing.Name = region.Name;
            await _regionRepository.Update(existing);
            return true;
        }

        public async Task<bool> DeleteRegion(int id, int farmerId)
        {
            Region? existing = await _regionRepository.GetByIdForFarmer(id, farmerId);
            if (existing == null)
            {
                return false;
            }

            await _regionRepository.Delete(existing);
            return true;
        }
    }
}
