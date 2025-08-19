using CropTrack.Models;
using CropTrack.Repositories;

namespace CropTrack.Services
{
    public class FieldService : IFieldService
    {
        private readonly IFieldRepository _fieldRepository;

        public FieldService(IFieldRepository fieldRepository)
        {
            _fieldRepository = fieldRepository;
        }

        public async Task<List<Field>> GetFarmerFields(int farmerId)
        {
            return await _fieldRepository.GetAllByFarmerId(farmerId);
        }

        public async Task<Field> GetFieldById(int fieldId, int farmerId)
        {
            var field = await _fieldRepository.GetById(fieldId);
            return (field != null && field.FarmerId == farmerId) ? field : null;
        }

        public async Task<bool> AddField(Field field, int farmerId)
        {
            field.FarmerId = farmerId;
            await _fieldRepository.Add(field);
            return true;
        }

        public async Task<bool> UpdateField(Field field, int farmerId)
        {
            var existing = await _fieldRepository.GetById(field.FieldId);
            if (existing == null || existing.FarmerId != farmerId)
                return false;

            existing.Name = field.Name;
            existing.Location = field.Location;
            existing.Acres = field.Acres;
            existing.RegionId = field.RegionId;

            await _fieldRepository.Update(existing);
            return true;
        }

        public async Task<bool> DeleteField(int fieldId, int farmerId)
        {
            var field = await _fieldRepository.GetById(fieldId);
            if (field == null || field.FarmerId != farmerId)
                return false;

            await _fieldRepository.Delete(field);
            return true;
        }
    }
}