using CropTrack.Models;

namespace CropTrack.Repositories
{
    public interface IFieldCropRepository
    {
        Task<List<FieldCrop>> GetAllByFieldId(int fieldId);
        Task<FieldCrop> GetById(int id);
        Task Add(FieldCrop fieldCrop);
        Task Update(FieldCrop fieldCrop);
        Task Delete(FieldCrop fieldCrop);
    }
}
