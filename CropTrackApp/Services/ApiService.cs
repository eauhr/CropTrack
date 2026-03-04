using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CropTrackApp.Models;

namespace CropTrackApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        private const string BaseUrl = "https://localhost:7115/api";

        public ApiService(AuthService authService)
        {
            _authService = authService;
            _httpClient = new HttpClient();
        }

       //JWT token

        private async Task AttachTokenAsync()
        {
            string token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

       
        private async Task<T> GetAsync<T>(string endpoint)
        {
            await AttachTokenAsync();
            HttpResponseMessage response = await _httpClient.GetAsync($"{BaseUrl}/{endpoint}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        private async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T body, bool requiresAuth = true)
        {
            if (requiresAuth)
                await AttachTokenAsync();

            string json = JsonSerializer.Serialize(body);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync($"{BaseUrl}/{endpoint}", content);
        }

        private async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T body)
        {
            await AttachTokenAsync();
            string json = JsonSerializer.Serialize(body);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PutAsync($"{BaseUrl}/{endpoint}", content);
        }

        private async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            await AttachTokenAsync();
            return await _httpClient.DeleteAsync($"{BaseUrl}/{endpoint}");
        }


        //authentication
        public async Task<bool> LoginAsync(string email, string password)
        {
            HttpResponseMessage response = await PostAsync("Farmer/login",
                new { Email = email, Password = password }, requiresAuth: false);

            if (!response.IsSuccessStatusCode)
                return false;

            JsonElement result = await response.Content.ReadFromJsonAsync<JsonElement>();
            string token = result.GetProperty("token").GetString();
            int farmerId = result.GetProperty("farmer").GetProperty("farmerId").GetInt32();
            string name = result.GetProperty("farmer").GetProperty("name").GetString();
            string farmerEmail = result.GetProperty("farmer").GetProperty("email").GetString();

            await _authService.SaveTokenAsync(token, farmerId, name, farmerEmail);
            return true;
        }
        //registation
        public async Task<bool> RegisterAsync(string name, string email, string password)
        {
            HttpResponseMessage response = await PostAsync("Farmer/register",
                new { Name = name, Email = email, Password = password }, requiresAuth: false);

            if (!response.IsSuccessStatusCode)
                return false;

            JsonElement result = await response.Content.ReadFromJsonAsync<JsonElement>();
            string token = result.GetProperty("token").GetString();
            int farmerId = result.GetProperty("farmer").GetProperty("farmerId").GetInt32();
            string farmerName = result.GetProperty("farmer").GetProperty("name").GetString();
            string farmerEmail = result.GetProperty("farmer").GetProperty("email").GetString();

            await _authService.SaveTokenAsync(token, farmerId, farmerName, farmerEmail);
            return true;
        }

        //models
        public async Task<List<Field>> GetFieldsAsync(int farmerId)
        {
            return await GetAsync<List<Field>>($"Field/{farmerId}");
        }

        public async Task<Field> GetFieldAsync(int farmerId, int fieldId)
        {
            return await GetAsync<Field>($"Field/{farmerId}/{fieldId}");
        }

        public async Task<bool> AddFieldAsync(string name, string location, decimal acres, int regionId)
        {
            HttpResponseMessage response = await PostAsync("Field/add",
                new { Name = name, Location = location, Acres = acres, RegionId = regionId });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateFieldAsync(int fieldId, string name, string location, decimal acres, int regionId)
        {
            HttpResponseMessage response = await PutAsync("Field",
                new { FieldId = fieldId, Name = name, Location = location, Acres = acres, RegionId = regionId });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteFieldAsync(int farmerId, int fieldId)
        {
            HttpResponseMessage response = await DeleteAsync($"Field/{farmerId}/{fieldId}");
            return response.IsSuccessStatusCode;
        }

        // ── Crops ───────────────────────────────────────────────────────────

        public async Task<List<Crop>> GetCropsAsync()
        {
            return await GetAsync<List<Crop>>("Crop");
        }

        public async Task<Crop> GetCropAsync(int id)
        {
            return await GetAsync<Crop>($"Crop/{id}");
        }

        public async Task<bool> AddCropAsync(string name, string unit, int avgGrowthDays, decimal yieldPerAcre, decimal optimalTemperature)
        {
            HttpResponseMessage response = await PostAsync("Crop",
                new { Name = name, Unit = unit, AvgGrowthDays = avgGrowthDays, YieldPerAcre = yieldPerAcre, OptimalTemperature = optimalTemperature });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCropAsync(int id)
        {
            HttpResponseMessage response = await DeleteAsync($"Crop/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<FieldCrop>> GetFieldCropsAsync(int fieldId)
        {
            return await GetAsync<List<FieldCrop>>($"FieldCrop/{fieldId}");
        }

        public async Task<bool> AddFieldCropAsync(int fieldId, int cropId, decimal quantityInTons, DateTime plantingDate, DateTime harvestDate)
        {
            HttpResponseMessage response = await PostAsync("FieldCrop/add",
                new { FieldId = fieldId, CropId = cropId, QuantityInTons = quantityInTons, PlantingDate = plantingDate, HarvestDate = harvestDate });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteFieldCropAsync(int id)
        {
            HttpResponseMessage response = await DeleteAsync($"FieldCrop/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<CropRegion>> GetRegionsAsync()
        {
            return await GetAsync<List<CropRegion>>("Region");
        }

        public async Task<List<WeatherLog>> GetWeatherLogsAsync(int regionId)
        {
            return await GetAsync<List<WeatherLog>>($"WeatherLog/{regionId}");
        }

        public async Task<List<MarketPrice>> GetMarketPricesAsync(int cropId)
        {
            return await GetAsync<List<MarketPrice>>($"MarketPrice/{cropId}");
        }
    }
}
