using CropTrack.Models;
using CropTrack.Repositories;
using Microsoft.AspNetCore.Identity;

namespace CropTrack.Services
{
    public class FarmerService
    {
        private FarmerRepository farmerRepository;
        private PasswordHasher<Farmer> passwordHasher;

        public FarmerService(FarmerRepository repo)
        {
            farmerRepository = repo;
            passwordHasher = new PasswordHasher<Farmer>();
        }

        public async Task<bool> RegisterAsync(RegisterFarmerRequest request)
        {
            Farmer existingFarmer = await farmerRepository.GetByEmailAsync(request.Email);

            if (existingFarmer != null)
            {
                throw new Exception("Email is already registered.");
            }

            Farmer farmer = new Farmer();
            farmer.Name = request.Name;
            farmer.Email = request.Email;
            
            farmer.Password = passwordHasher.HashPassword(farmer, request.Password);
            await farmerRepository.AddAsync(farmer);
            return true;
        }
        public async Task<Farmer> LoginAsync(LoginFarmerRequest request)
        {
            Farmer farmer = await farmerRepository.GetByEmailAsync(request.Email);

            if (farmer == null)
            {
                throw new Exception("Invalid email or password.");
            }

            PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(
                farmer,
                farmer.Password,
                request.Password
            );
            if (result == PasswordVerificationResult.Failed)
            {
                throw new Exception("Invalid email or password.");
            }
            return farmer;
        }
    }
}
