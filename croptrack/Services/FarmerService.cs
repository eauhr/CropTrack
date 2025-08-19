using CropTrack.Models;
using CropTrack.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace CropTrack.Services
{
    public class FarmerService : IFarmerService
    {
        private readonly IFarmerRepository _farmerRepository;

        public FarmerService(IFarmerRepository farmerRepository)
        {
            _farmerRepository = farmerRepository;
        }

        public async Task<bool> Register(RegisterFarmerRequest request)
        {
            Farmer existingFarmer = await _farmerRepository.GetByEmail(request.Email);
            if (existingFarmer != null)
                return false;

            var passwordHasher = HashPassword(request.Password);

            Farmer farmer = new Farmer
            {
                Name = request.Name,
                Email = request.Email,
                Password = passwordHasher
            };

            await _farmerRepository.Add(farmer);
            return true;
        }
        public async Task<Farmer> Login(LoginFarmerRequest request)
        {
            Farmer farmer = await _farmerRepository.GetByEmail(request.Email);
            if (farmer == null || !VerifyPassword(request.Password, farmer.Password))
                throw new Exception("Invalid email or password");

            return farmer;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public async Task<Farmer> RegisterAndReturnFarmer(RegisterFarmerRequest request)
        {
            Farmer existingFarmer = await _farmerRepository.GetByEmail(request.Email);
            if (existingFarmer != null)
                throw new Exception("Farmer already exists");

            var passwordHasher = HashPassword(request.Password);

            Farmer farmer = new Farmer
            {
                Name = request.Name,
                Email = request.Email,
                Password = passwordHasher
            };

            await _farmerRepository.Add(farmer);
            return farmer;
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            return HashPassword(inputPassword) == storedHash;
        }


    }
}
