using CropTrack.Models;
using CropTrack.Repositories;

namespace CropTrack.Services
{
    public class CropService : ICropService
    {
        private readonly ICropRepository _repository;

        public CropService(ICropRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Crop>> GetAllCrops(int farmerId)
        {
            return await _repository.GetAllByFarmerId(farmerId);
        }

        public async Task<Crop?> GetCropById(int id, int farmerId)
        {
            return await _repository.GetByIdForFarmer(id, farmerId);
        }

        public async Task<int> AddCrop(Crop crop, int farmerId)
        {
            crop.FarmerId = farmerId;
            await _repository.Add(crop);
            return crop.CropId;
        }

        public async Task<bool> UpdateCrop(Crop crop, int farmerId)
        {
            Crop? existing = await _repository.GetByIdForFarmer(crop.CropId, farmerId);
            if (existing == null)
            {
                return false;
            }

            existing.Name = crop.Name;
            existing.Unit = crop.Unit;
            existing.AvgGrowthDays = crop.AvgGrowthDays;
            existing.YieldPerAcre = crop.YieldPerAcre;
            existing.OptimalTemperature = crop.OptimalTemperature;

            await _repository.Update(existing);
            return true;
        }

        public async Task<bool> DeleteCrop(int id, int farmerId)
        {
            Crop? existing = await _repository.GetByIdForFarmer(id, farmerId);
            if (existing == null)
            {
                return false;
            }

            await _repository.Delete(existing);
            return true;
        }
    }
}
