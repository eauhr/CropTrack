using System.Globalization;
using System.Text.Json;

namespace CropTrack.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<double?> GetCurrentTemperatureAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default)
        {
            string url =
                "https://api.open-meteo.com/v1/forecast" +
                $"?latitude={latitude.ToString(CultureInfo.InvariantCulture)}" +
                $"&longitude={longitude.ToString(CultureInfo.InvariantCulture)}" +
                "&current=temperature_2m";

            using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("current", out JsonElement current) ||
                !current.TryGetProperty("temperature_2m", out JsonElement temperatureNode))
            {
                return null;
            }

            return temperatureNode.GetDouble();
        }
    }
}
