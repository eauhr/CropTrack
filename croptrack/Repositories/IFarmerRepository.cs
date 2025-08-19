using CropTrack.Models;

namespace CropTrack.Repositories
{
    public interface IFarmerRepository
    {
        Task<Farmer> GetByEmail(string email);
        Task Add(Farmer farmer);
    }
}
