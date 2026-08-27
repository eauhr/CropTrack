using CropTrack.Models;

namespace CropTrack.Services
{
    public interface IFarmerService
    {
        Task<Farmer> Login(LoginFarmerRequest request);
        Task<Farmer> RegisterAndReturnFarmer(RegisterFarmerRequest request);
    }
}
