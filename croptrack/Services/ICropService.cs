using CropTrack.Models;

namespace CropTrack.Services
{
    public interface ICropService
    {
        Task<List<Crop>> GetAllCrops(int farmerId);
        Task<Crop?> GetCropById(int id, int farmerId);
        Task<int> AddCrop(Crop crop, int farmerId);
        Task<bool> UpdateCrop(Crop crop, int farmerId);
        Task<bool> DeleteCrop(int id, int farmerId);
    }
}
