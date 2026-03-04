using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CropTrackApp.Services
{
    public class AuthService
    {
        private const string TokenKey = "jwt_token";
        private const string FarmerIdKey = "farmer_id";
        private const string FarmerNameKey = "farmer_name";
        private const string FarmerEmailKey = "farmer_email";

        public async Task SaveTokenAsync(string token, int farmerId, string name, string email)
        {
            await SecureStorage.SetAsync(TokenKey, token);
            await SecureStorage.SetAsync(FarmerIdKey, farmerId.ToString());
            await SecureStorage.SetAsync(FarmerNameKey, name);
            await SecureStorage.SetAsync(FarmerEmailKey, email);
        }

        public async Task<string> GetTokenAsync()
        {
            return await SecureStorage.GetAsync(TokenKey);
        }

        public async Task<int> GetFarmerIdAsync()
        {
            string value = await SecureStorage.GetAsync(FarmerIdKey);
            return int.TryParse(value, out int id) ? id : 0;
        }

        public async Task<string> GetFarmerNameAsync()
        {
            return await SecureStorage.GetAsync(FarmerNameKey);
        }

        public async Task<string> GetFarmerEmailAsync()
        {
            return await SecureStorage.GetAsync(FarmerEmailKey);
        }

        public async Task<bool> IsLoggedInAsync()
        {
            string token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return false;

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo > DateTime.UtcNow;
        }

        public void Logout()
        {
            SecureStorage.Remove(TokenKey);
            SecureStorage.Remove(FarmerIdKey);
            SecureStorage.Remove(FarmerNameKey);
            SecureStorage.Remove(FarmerEmailKey);
        }
    }
}