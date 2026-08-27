using CropTrack.Models;

namespace CropTrack.Services
{
    public interface IFieldService
    {
        Task<List<Field>> GetFarmerFields(int farmerId);
        Task<Field> GetFieldById(int fieldId, int farmerId);
        Task<bool> AddField(Field field, int farmerId);
        Task<bool> UpdateField(Field field, int farmerId);
        Task<bool> DeleteField(int fieldId, int farmerId);
    }
}
