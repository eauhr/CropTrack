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

        public async Task<IEnumerable<Crop>> GetAllCrops()
        {
            return await _repository.GetAll();
        }

        public async Task<Crop> GetCropById(int id)
        {
            return await _repository.GetById(id);
        }

        public async Task AddCrop(Crop crop)
        {
            await _repository.Add(crop);
        }

        public async Task UpdateCrop(Crop crop)
        {
            await _repository.Update(crop);
        }

        public async Task DeleteCrop(int id)
        {
            Crop crop = await _repository.GetById(id);
            if (crop != null)
            {
                await _repository.Delete(crop);
            }
        }
    }
}
