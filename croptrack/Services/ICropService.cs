using CropTrack.Models;

namespace CropTrack.Services
{
    public interface ICropService
    {
        Task<IEnumerable<Crop>> GetAllCrops();
        Task<Crop> GetCropById(int id);
        Task AddCrop(Crop crop);
        Task UpdateCrop(Crop crop);
        Task DeleteCrop(int id);
    }
}
