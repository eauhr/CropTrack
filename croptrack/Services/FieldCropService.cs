using CropTrack.Models;
using CropTrack.Repositories;

namespace CropTrack.Services
{
    public class FieldCropService : IFieldCropService
    {
        private readonly IFieldCropRepository _fieldCropRepository;
        private readonly IFieldRepository _fieldRepository;

        public FieldCropService(IFieldCropRepository fieldCropRepository, IFieldRepository fieldRepository)
        {
            _fieldCropRepository = fieldCropRepository;
            _fieldRepository = fieldRepository;
        }

        public async Task<List<FieldCrop>> GetFieldCrops(int fieldId)
        {
            return await _fieldCropRepository.GetAllByFieldId(fieldId);
        }

        public async Task<FieldCrop> GetFieldCropById(int id)
        {
            return await _fieldCropRepository.GetById(id);
        }

        public async Task<bool> AddFieldCrop(FieldCrop fieldCrop)
        {
            var field = await _fieldRepository.GetById(fieldCrop.FieldId);
            if (field == null)
                return false;

            await _fieldCropRepository.Add(fieldCrop);
            return true;
        }

        public async Task<bool> UpdateFieldCrop(FieldCrop fieldCrop)
        {
            var existing = await _fieldCropRepository.GetById(fieldCrop.FieldCropId);
            if (existing == null)
                return false;

            existing.CropId = fieldCrop.CropId;
            existing.QuantityInTons = fieldCrop.QuantityInTons;
            existing.PlantingDate = fieldCrop.PlantingDate;
            existing.HarvestDate = fieldCrop.HarvestDate;

            await _fieldCropRepository.Update(existing);
            return true;
        }

        public async Task<bool> DeleteFieldCrop(int id)
        {
            var fieldCrop = await _fieldCropRepository.GetById(id);
            if (fieldCrop == null)
                return false;

            await _fieldCropRepository.Delete(fieldCrop);
            return true;
        }
    }
}
