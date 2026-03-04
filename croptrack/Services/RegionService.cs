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

        public async Task<List<Region>> GetAllRegions()
        {
            return await _regionRepository.GetAll();
        }

        public async Task<Region> GetRegionById(int id)
        {
            return await _regionRepository.GetById(id);
        }

        public async Task AddRegion(Region region)
        {
            await _regionRepository.Add(region);
        }

        public async Task UpdateRegion(Region region)
        {
            await _regionRepository.Update(region);
        }

        public async Task<bool> DeleteRegion(int id)
        {
            Region region = await _regionRepository.GetById(id);
            if (region == null)
                return false;

            await _regionRepository.Delete(region);
            return true;
        }
    }
}
