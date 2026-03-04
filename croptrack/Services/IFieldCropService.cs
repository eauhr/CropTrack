using CropTrack.Models;

namespace CropTrack.Services
{
    public interface IFieldCropService
    {
        Task<List<FieldCrop>> GetFieldCrops(int fieldId);
        Task<FieldCrop> GetFieldCropById(int id);
        Task<bool> AddFieldCrop(FieldCrop fieldCrop);
        Task<bool> UpdateFieldCrop(FieldCrop fieldCrop);
        Task<bool> DeleteFieldCrop(int id);
    }
}
