using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Linq;
using CropTrackApp.Models;

namespace CropTrackApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;
        private readonly string[] _baseUrls;
        private string _activeBaseUrl;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public int LastStatusCode { get; private set; }
        public string? LastError { get; private set; }

        public ApiService(AuthService authService)
        {
            _authService = authService;
            _baseUrls = GetBaseUrls();
            _activeBaseUrl = _baseUrls[0];

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    if (cert?.Issuer.Equals("CN=localhost") == true)
                    {
                        return true;
                    }

                    return errors == SslPolicyErrors.None;
                }
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(60)
            };
        }

        private static bool IsRetryableNetworkException(Exception ex)
        {
            return ex is TaskCanceledException || ex is HttpRequestException;
        }

        private async Task<HttpResponseMessage> ExecuteWithRetryAsync(Func<Task<HttpResponseMessage>> action, int maxAttempts = 2)
        {
            Exception? lastException = null;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (IsRetryableNetworkException(ex))
                {
                    lastException = ex;
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(1200);
                    }
                }
            }

            throw lastException ?? new HttpRequestException("Network request failed.");
        }

        private static string[] GetBaseUrls()
        {
            return DeviceInfo.Platform == DevicePlatform.Android
                ? new[]
                {
                    "http://10.0.2.2:5075/api/",
                    "http://10.0.3.2:5075/api/"
                }
                : new[]
                {
                    "http://localhost:5075/api/",
                    "http://127.0.0.1:5075/api/"
                };
        }

        private async Task<bool> EnsureReachableBaseUrlAsync()
        {
            IEnumerable<string> ordered = _baseUrls
                .OrderByDescending(u => string.Equals(u, _activeBaseUrl, StringComparison.OrdinalIgnoreCase));

            foreach (string baseUrl in ordered)
            {
                if (await CanReachBaseUrlAsync(baseUrl))
                {
                    _activeBaseUrl = baseUrl;
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> CanReachBaseUrlAsync(string baseUrl)
        {
            try
            {
                using HttpClient probeClient = new() { Timeout = TimeSpan.FromSeconds(4) };
                using HttpResponseMessage response = await probeClient.GetAsync(baseUrl);
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        private string BuildUrl(string endpoint)
        {
            return $"{_activeBaseUrl}{endpoint}";
        }

        private async Task AttachTokenAsync()
        {
            string? token = await _authService.GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<T> GetAsync<T>(string endpoint)
        {
            await AttachTokenAsync();
            using HttpResponseMessage response = await _httpClient.GetAsync(BuildUrl(endpoint));
            response.EnsureSuccessStatusCode();

            T? result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            if (result is null)
            {
                throw new InvalidOperationException($"Empty response body for endpoint '{endpoint}'.");
            }

            return result;
        }

        private async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T body, bool requiresAuth = true)
        {
            if (requiresAuth)
            {
                await AttachTokenAsync();
            }

            string json = JsonSerializer.Serialize(body, JsonOptions);
            using StringContent content = new(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(BuildUrl(endpoint), content);
        }

        private async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T body)
        {
            await AttachTokenAsync();
            string json = JsonSerializer.Serialize(body, JsonOptions);
            using StringContent content = new(json, Encoding.UTF8, "application/json");
            return await _httpClient.PutAsync(BuildUrl(endpoint), content);
        }

        private async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            await AttachTokenAsync();
            return await _httpClient.DeleteAsync(BuildUrl(endpoint));
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            bool reachable = await EnsureReachableBaseUrlAsync();
            if (!reachable)
            {
                LastStatusCode = 0;
                LastError = "Cannot reach API server. Start backend and confirm it is listening on port 5075.";
                return false;
            }

            HttpResponseMessage response;
            try
            {
                response = await ExecuteWithRetryAsync(() =>
                    PostAsync("Farmer/login", new { Email = email, Password = password }, requiresAuth: false));
            }
            catch (TaskCanceledException)
            {
                LastStatusCode = 0;
                LastError = "Request timed out. Make sure the API is running and reachable at http://10.0.2.2:5075, then try again.";
                return false;
            }
            catch (HttpRequestException)
            {
                LastStatusCode = 0;
                LastError = "Cannot reach server. Start the API project and check emulator network.";
                return false;
            }

            LastStatusCode = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                LastError = await ExtractErrorMessageAsync(response);
                return false;
            }

            JsonElement result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            if (!TryExtractAuthPayload(result, out string token, out int farmerId, out string farmerName, out string farmerEmail))
            {
                LastError = "Received an unexpected response from the server.";
                return false;
            }

            await _authService.SaveTokenAsync(token, farmerId, farmerName, farmerEmail);
            LastError = null;
            return true;
        }

        public async Task<bool> RegisterAsync(string name, string email, string password)
        {
            bool reachable = await EnsureReachableBaseUrlAsync();
            if (!reachable)
            {
                LastStatusCode = 0;
                LastError = "Cannot reach API server. Start backend and confirm it is listening on port 5075.";
                return false;
            }

            HttpResponseMessage response;
            try
            {
                response = await ExecuteWithRetryAsync(() =>
                    PostAsync("Farmer/register", new { Name = name, Email = email, Password = password }, requiresAuth: false));
            }
            catch (TaskCanceledException)
            {
                LastStatusCode = 0;
                LastError = "Request timed out. Make sure the API is running and reachable at http://10.0.2.2:5075, then try again.";
                return false;
            }
            catch (HttpRequestException)
            {
                LastStatusCode = 0;
                LastError = "Cannot reach server. Start the API project and check emulator network.";
                return false;
            }

            LastStatusCode = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                LastError = await ExtractErrorMessageAsync(response);
                return false;
            }

            JsonElement result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            if (!TryExtractAuthPayload(result, out string token, out int farmerId, out string farmerName, out string farmerEmail))
            {
                LastError = "Received an unexpected response from the server.";
                return false;
            }

            await _authService.SaveTokenAsync(token, farmerId, farmerName, farmerEmail);
            LastError = null;
            return true;
        }

        private static bool TryExtractAuthPayload(JsonElement result, out string token, out int farmerId, out string farmerName, out string farmerEmail)
        {
            token = string.Empty;
            farmerId = 0;
            farmerName = string.Empty;
            farmerEmail = string.Empty;

            if (!result.TryGetProperty("token", out JsonElement tokenElement))
            {
                return false;
            }

            if (!result.TryGetProperty("farmer", out JsonElement farmerElement))
            {
                return false;
            }

            token = tokenElement.GetString() ?? string.Empty;
            farmerId = farmerElement.TryGetProperty("farmerId", out JsonElement idElement) ? idElement.GetInt32() : 0;
            farmerName = farmerElement.TryGetProperty("name", out JsonElement nameElement) ? nameElement.GetString() ?? string.Empty : string.Empty;
            farmerEmail = farmerElement.TryGetProperty("email", out JsonElement emailElement) ? emailElement.GetString() ?? string.Empty : string.Empty;

            return !string.IsNullOrWhiteSpace(token);
        }

        private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
        {
            string raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return $"Request failed with status {(int)response.StatusCode}.";
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(raw);
                JsonElement root = doc.RootElement;

                if (TryGetString(root, "message", out string message))
                {
                    return message;
                }

                if (TryGetString(root, "detail", out string detail))
                {
                    return detail;
                }

                if (TryGetString(root, "title", out string title))
                {
                    return title;
                }

                if (root.TryGetProperty("errors", out JsonElement errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    foreach (JsonProperty errorEntry in errors.EnumerateObject())
                    {
                        if (errorEntry.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (JsonElement item in errorEntry.Value.EnumerateArray())
                            {
                                string? itemValue = item.GetString();
                                if (!string.IsNullOrWhiteSpace(itemValue))
                                {
                                    return itemValue;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // raw may be plain text. Fall back below.
            }

            return raw;
        }

        private static bool TryGetString(JsonElement root, string propertyName, out string value)
        {
            value = string.Empty;
            if (!root.TryGetProperty(propertyName, out JsonElement property))
            {
                return false;
            }

            value = property.GetString() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(value);
        }

        public async Task<List<Field>> GetFieldsAsync(int farmerId)
        {
            return await GetAsync<List<Field>>($"Field/{farmerId}");
        }

        public async Task<Field> GetFieldAsync(int farmerId, int fieldId)
        {
            return await GetAsync<Field>($"Field/{farmerId}/{fieldId}");
        }

        public async Task<bool> AddFieldAsync(string name, string location, decimal acres, int regionId = 0)
        {
            HttpResponseMessage response = await PostAsync("Field/add", new { Name = name, Location = location, Acres = acres, RegionId = regionId });
            LastStatusCode = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                LastError = await ExtractErrorMessageAsync(response);
                return false;
            }

            LastError = null;
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateFieldAsync(int fieldId, string name, string location, decimal acres, int regionId = 0)
        {
            HttpResponseMessage response = await PutAsync("Field", new { FieldId = fieldId, Name = name, Location = location, Acres = acres, RegionId = regionId });
            LastStatusCode = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                LastError = await ExtractErrorMessageAsync(response);
                return false;
            }

            LastError = null;
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteFieldAsync(int farmerId, int fieldId)
        {
            HttpResponseMessage response = await DeleteAsync($"Field/{farmerId}/{fieldId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Crop>> GetCropsAsync()
        {
            return await GetAsync<List<Crop>>("Crop");
        }

        public async Task<List<ProduceKnowledge>> GetProduceCatalogAsync()
        {
            return await GetAsync<List<ProduceKnowledge>>("Produce");
        }

        public async Task<List<ProduceKnowledge>> GetRecommendedProducesAsync(double currentTemp)
        {
            string temp = currentTemp.ToString("0.##", CultureInfo.InvariantCulture);
            return await GetAsync<List<ProduceKnowledge>>($"Produce/recommended?currentTemp={temp}");
        }

        public async Task<Crop> GetCropAsync(int id)
        {
            return await GetAsync<Crop>($"Crop/{id}");
        }

        public async Task<bool> AddCropAsync(string name, string unit, int avgGrowthDays, decimal yieldPerAcre, decimal optimalTemperature)
        {
            HttpResponseMessage response = await PostAsync("Crop", new { Name = name, Unit = unit, AvgGrowthDays = avgGrowthDays, YieldPerAcre = yieldPerAcre, OptimalTemperature = optimalTemperature });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCropAsync(int cropId, string name, string unit, int avgGrowthDays, decimal yieldPerAcre, decimal optimalTemperature)
        {
            HttpResponseMessage response = await PutAsync($"Crop/{cropId}", new
            {
                CropId = cropId,
                Name = name,
                Unit = unit,
                AvgGrowthDays = avgGrowthDays,
                YieldPerAcre = yieldPerAcre,
                OptimalTemperature = optimalTemperature
            });
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
            HttpResponseMessage response = await PostAsync("FieldCrop/add", new { FieldId = fieldId, CropId = cropId, QuantityInTons = quantityInTons, PlantingDate = plantingDate, HarvestDate = harvestDate });
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

        public async Task<List<LocationSuggestion>> SearchLocationSuggestionsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<LocationSuggestion>();
            }

            try
            {
                using HttpClient geocodingClient = new() { Timeout = TimeSpan.FromSeconds(8) };
                string url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(query.Trim())}&count=6&language=en&format=json";
                using HttpResponseMessage response = await geocodingClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return new List<LocationSuggestion>();
                }

                OpenMeteoGeocodingResponse? data = await response.Content.ReadFromJsonAsync<OpenMeteoGeocodingResponse>(JsonOptions);
                if (data?.Results is null || data.Results.Count == 0)
                {
                    return new List<LocationSuggestion>();
                }

                return data.Results
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .Select(x => new LocationSuggestion
                    {
                        Name = x.Name ?? string.Empty,
                        Admin1 = x.Admin1 ?? string.Empty,
                        Country = x.Country ?? string.Empty,
                        Latitude = x.Latitude,
                        Longitude = x.Longitude
                    })
                    .ToList();
            }
            catch
            {
                return new List<LocationSuggestion>();
            }
        }

        public async Task<List<WeatherLog>> GetWeatherLogsAsync(int regionId)
        {
            return await GetAsync<List<WeatherLog>>($"WeatherLog/{regionId}");
        }

        public async Task<List<MarketPrice>> GetMarketPricesAsync(int cropId)
        {
            return await GetAsync<List<MarketPrice>>($"MarketPrice/{cropId}");
        }

        public async Task<bool> AddMarketPriceAsync(int cropId, decimal pricePerTon, DateTime dateRecorded)
        {
            HttpResponseMessage response = await PostAsync("MarketPrice", new
            {
                CropId = cropId,
                PricePerTon = pricePerTon,
                DateRecorded = dateRecorded
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateMarketPriceAsync(int marketPriceId, int cropId, decimal pricePerTon, DateTime dateRecorded)
        {
            HttpResponseMessage response = await PutAsync($"MarketPrice/{marketPriceId}", new
            {
                CropId = cropId,
                PricePerTon = pricePerTon,
                DateRecorded = dateRecorded
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMarketPriceAsync(int marketPriceId)
        {
            HttpResponseMessage response = await DeleteAsync($"MarketPrice/{marketPriceId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> FetchAndStoreUsdaPriceAsync(int cropId)
        {
            HttpResponseMessage response = await PostAsync($"MarketPrice/fetch-usda/{cropId}", new { });
            LastStatusCode = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                LastError = await ExtractErrorMessageAsync(response);
                return false;
            }

            LastError = null;
            return true;
        }

        public async Task<bool> AddWeatherLogAsync(int regionId, DateTime dateRecorded, decimal temperature, decimal rainfall, string forecast)
        {
            HttpResponseMessage response = await PostAsync("WeatherLog", new
            {
                RegionId = regionId,
                DateRecorded = dateRecorded,
                Temperature = temperature,
                Rainfall = rainfall,
                Forecast = forecast
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateWeatherLogAsync(int weatherLogId, int regionId, DateTime dateRecorded, decimal temperature, decimal rainfall, string forecast)
        {
            HttpResponseMessage response = await PutAsync($"WeatherLog/{weatherLogId}", new
            {
                RegionId = regionId,
                DateRecorded = dateRecorded,
                Temperature = temperature,
                Rainfall = rainfall,
                Forecast = forecast
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteWeatherLogAsync(int weatherLogId)
        {
            HttpResponseMessage response = await DeleteAsync($"WeatherLog/{weatherLogId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> FetchAndStoreOpenMeteoWeatherAsync(int regionId)
        {
            HttpResponseMessage response = await PostAsync($"WeatherLog/fetch-open-meteo/{regionId}", new { });
            LastStatusCode = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                LastError = await ExtractErrorMessageAsync(response);
                return false;
            }

            LastError = null;
            return true;
        }

        public async Task<bool> AddRegionAsync(string name)
        {
            HttpResponseMessage response = await PostAsync("Region", new { Name = name });
            return response.IsSuccessStatusCode;
        }

        private sealed class OpenMeteoGeocodingResponse
        {
            public List<OpenMeteoGeocodingItem>? Results { get; set; }
        }

        private sealed class OpenMeteoGeocodingItem
        {
            public string? Name { get; set; }
            public string? Admin1 { get; set; }
            public string? Country { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
}
