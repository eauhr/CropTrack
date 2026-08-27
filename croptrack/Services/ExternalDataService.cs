using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace CropTrack.Services
{
    public class ExternalDataService : IExternalDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ExternalDataService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<OpenMeteoWeatherResult?> FetchOpenMeteoWeatherAsync(string regionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(regionName))
            {
                return null;
            }

            string geocodeUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(regionName)}&count=1&language=en&format=json";
            using HttpResponseMessage geoResponse = await _httpClient.GetAsync(geocodeUrl, cancellationToken);
            geoResponse.EnsureSuccessStatusCode();

            string geoJson = await geoResponse.Content.ReadAsStringAsync(cancellationToken);
            using JsonDocument geoDoc = JsonDocument.Parse(geoJson);
            JsonElement geoRoot = geoDoc.RootElement;

            if (!geoRoot.TryGetProperty("results", out JsonElement results) ||
                results.ValueKind != JsonValueKind.Array ||
                results.GetArrayLength() == 0)
            {
                return null;
            }

            JsonElement first = results[0];
            double latitude = first.GetProperty("latitude").GetDouble();
            double longitude = first.GetProperty("longitude").GetDouble();

            string forecastUrl =
                "https://api.open-meteo.com/v1/forecast" +
                $"?latitude={latitude.ToString(CultureInfo.InvariantCulture)}" +
                $"&longitude={longitude.ToString(CultureInfo.InvariantCulture)}" +
                "&current=temperature_2m,relative_humidity_2m,precipitation,weather_code" +
                "&daily=temperature_2m_max,temperature_2m_min,precipitation_sum,weather_code" +
                "&timezone=auto&forecast_days=3";

            using HttpResponseMessage weatherResponse = await _httpClient.GetAsync(forecastUrl, cancellationToken);
            weatherResponse.EnsureSuccessStatusCode();

            string weatherJson = await weatherResponse.Content.ReadAsStringAsync(cancellationToken);
            using JsonDocument weatherDoc = JsonDocument.Parse(weatherJson);
            JsonElement weatherRoot = weatherDoc.RootElement;

            JsonElement current = weatherRoot.GetProperty("current");
            JsonElement daily = weatherRoot.GetProperty("daily");

            decimal temperature = Convert.ToDecimal(current.GetProperty("temperature_2m").GetDouble());
            decimal currentPrecipitation = Convert.ToDecimal(current.GetProperty("precipitation").GetDouble());
            int weatherCode = current.GetProperty("weather_code").GetInt32();

            decimal dailyMax = GetArrayDecimal(daily, "temperature_2m_max", 0);
            decimal dailyMin = GetArrayDecimal(daily, "temperature_2m_min", 0);
            decimal dailyPrecipitation = GetArrayDecimal(daily, "precipitation_sum", 0);
            decimal rainfall = dailyPrecipitation > 0 ? dailyPrecipitation : currentPrecipitation;

            string forecast = $"{MapWeatherCode(weatherCode)} • {dailyMin:0.#}C to {dailyMax:0.#}C";

            return new OpenMeteoWeatherResult
            {
                RecordedAt = DateTime.UtcNow,
                TemperatureC = temperature,
                RainfallMm = rainfall,
                Forecast = forecast,
                DailyMaxC = dailyMax,
                DailyMinC = dailyMin,
                Latitude = latitude,
                Longitude = longitude
            };
        }

        public async Task<UsdaPriceResult?> FetchUsdaLatestPriceAsync(string cropName, CancellationToken cancellationToken = default)
        {
            string? apiKey = _configuration["USDA:ApiKey"] ?? Environment.GetEnvironmentVariable("USDA_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("USDA API key is missing. Set USDA:ApiKey or USDA_API_KEY.");
            }

            string commodity = MapCommodityName(cropName);
            int[] years = new[] { DateTime.UtcNow.Year, DateTime.UtcNow.Year - 1 };

            foreach (int year in years)
            {
                string url =
                    "https://quickstats.nass.usda.gov/api/api_GET/" +
                    $"?key={Uri.EscapeDataString(apiKey)}" +
                    $"&commodity_desc={Uri.EscapeDataString(commodity)}" +
                    "&statisticcat_desc=PRICE%20RECEIVED" +
                    "&agg_level_desc=NATIONAL" +
                    $"&year={year}" +
                    "&format=JSON";

                using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync(cancellationToken);
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;
                if (!root.TryGetProperty("data", out JsonElement data) ||
                    data.ValueKind != JsonValueKind.Array ||
                    data.GetArrayLength() == 0)
                {
                    continue;
                }

                foreach (JsonElement item in data.EnumerateArray())
                {
                    if (!item.TryGetProperty("Value", out JsonElement valueProp))
                    {
                        continue;
                    }

                    string rawValue = valueProp.GetString() ?? string.Empty;
                    decimal? parsed = ParseUsdaValue(rawValue);
                    if (!parsed.HasValue)
                    {
                        continue;
                    }

                    string unit = item.TryGetProperty("unit_desc", out JsonElement unitProp)
                        ? (unitProp.GetString() ?? "$ / UNIT")
                        : "$ / UNIT";

                    return new UsdaPriceResult
                    {
                        Price = parsed.Value,
                        Unit = unit,
                        Year = year,
                        Commodity = commodity,
                        SourceNote = "USDA NASS QuickStats"
                    };
                }
            }

            return null;
        }

        private static decimal GetArrayDecimal(JsonElement root, string propertyName, int index)
        {
            if (!root.TryGetProperty(propertyName, out JsonElement array) ||
                array.ValueKind != JsonValueKind.Array ||
                array.GetArrayLength() <= index)
            {
                return 0m;
            }

            return Convert.ToDecimal(array[index].GetDouble());
        }

        private static decimal? ParseUsdaValue(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            string cleaned = raw.Replace(",", string.Empty).Trim();
            if (cleaned == "(D)" || cleaned == "(NA)" || cleaned == "(Z)")
            {
                return null;
            }

            return decimal.TryParse(cleaned, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.InvariantCulture, out decimal value)
                ? value
                : null;
        }

        private static string MapCommodityName(string cropName)
        {
            string name = (cropName ?? string.Empty).Trim().ToUpperInvariant();
            if (name.Contains("WHEAT"))
            {
                return "WHEAT";
            }

            if (name.Contains("CORN") || name.Contains("MAIZE"))
            {
                return "CORN";
            }

            if (name.Contains("SOY"))
            {
                return "SOYBEANS";
            }

            if (name.Contains("RICE"))
            {
                return "RICE";
            }

            if (name.Contains("BARLEY"))
            {
                return "BARLEY";
            }

            return name;
        }

        private static string MapWeatherCode(int code)
        {
            return code switch
            {
                0 => "Clear sky",
                1 => "Mainly clear",
                2 => "Partly cloudy",
                3 => "Overcast",
                45 or 48 => "Fog",
                51 or 53 or 55 => "Drizzle",
                61 or 63 or 65 => "Rain",
                71 or 73 or 75 => "Snow",
                80 or 81 or 82 => "Rain showers",
                95 or 96 or 99 => "Thunderstorm",
                _ => $"Weather code {code}"
            };
        }
    }
}
