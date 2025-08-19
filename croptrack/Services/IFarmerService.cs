using CropTrack.Models;

namespace CropTrack.Services
{
    public interface IFarmerService
    {
        Task<bool> Register(RegisterFarmerRequest request);
        Task<Farmer> Login(LoginFarmerRequest request);
        Task<Farmer> RegisterAndReturnFarmer(RegisterFarmerRequest request);
    }
}
