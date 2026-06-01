using CropTrackApp.Models;
using CropTrackApp.Services;

namespace CropTrackApp.Pages;

public partial class WeatherPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly List<CropRegion> _regions = new();
    private readonly List<WeatherLog> _logs = new();
    private bool _isLoading;

    public WeatherPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await EnsureRegionsLoadedAsync();
    }

    private async Task EnsureRegionsLoadedAsync()
    {
        try
        {
            _regions.Clear();
            _regions.AddRange(await _apiService.GetRegionsAsync());
            RegionPicker.ItemsSource = _regions;
            if (_regions.Count > 0 && RegionPicker.SelectedIndex < 0)
            {
                RegionPicker.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Weather Error", ex.Message, "OK");
        }
    }

    private async Task LoadWeatherAsync()
    {
        if (_isLoading)
        {
            return;
        }

        CropRegion? region = RegionPicker.SelectedItem as CropRegion;
        if (region is null)
        {
            await DisplayAlert("Select Region", "Please select a region first.", "OK");
            return;
        }

        _isLoading = true;
        try
        {
            List<WeatherLog> logs = await _apiService.GetWeatherLogsAsync(region.RegionId);
            _logs.Clear();
            _logs.AddRange(logs.OrderByDescending(w => w.DateRecorded));
            BindWeatherCards(region);
            WeatherCollectionView.ItemsSource = _logs.Select(ToLogCard).ToList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Load Failed", ex.Message, "OK");
        }
        finally
        {
            _isLoading = false;
            WeatherRefreshView.IsRefreshing = false;
        }
    }

    private void BindWeatherCards(CropRegion region)
    {
        if (_logs.Count == 0)
        {
            CurrentSummaryLabel.Text = $"Current ({region.Name}): no logs yet";
            RainfallSummaryLabel.Text = "Rainfall: N/A";
            ForecastSummaryLabel.Text = "Forecast: N/A";
            CriticalSummaryLabel.Text = "Critical alerts: none";
            return;
        }

        WeatherLog latest = _logs[0];
        CurrentSummaryLabel.Text = $"Current ({region.Name}): {MeasurementService.FormatTemperature(latest.Temperature)}";
        RainfallSummaryLabel.Text = $"Rainfall: {MeasurementService.FormatRainfall(latest.Rainfall)}";
        ForecastSummaryLabel.Text = $"Forecast: {latest.Forecast}";

        WeatherLog? critical = _logs.FirstOrDefault(w => IsCritical(w.Forecast));
        CriticalSummaryLabel.Text = critical is null
            ? "Critical alerts: none"
            : $"Critical alerts: {critical.Forecast}";
    }

    private async Task AddOrEditWeatherAsync(WeatherLog? existing)
    {
        CropRegion? region = RegionPicker.SelectedItem as CropRegion;
        if (region is null)
        {
            await DisplayAlert("Select Region", "Select region before adding weather logs.", "OK");
            return;
        }

        bool isEdit = existing is not null;
        string title = isEdit ? "Edit Weather Log" : "Add Weather Log";

        string dateDefault = existing?.DateRecorded.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
        string? dateText = await DisplayPromptAsync(title, "Date (yyyy-MM-dd)", initialValue: dateDefault);
        if (!DateTime.TryParse(dateText, out DateTime dateRecorded))
        {
            await DisplayAlert("Validation", "Date format should be yyyy-MM-dd.", "OK");
            return;
        }

        string tempUnit = MeasurementService.TemperatureUnit;
        string tempDefault = existing is null ? "20" : MeasurementService.FromCelsius(existing.Temperature).ToString("0.#");
        string? tempText = await DisplayPromptAsync(title, $"Temperature ({tempUnit})", initialValue: tempDefault, keyboard: Keyboard.Numeric);
        if (!decimal.TryParse(tempText, out decimal temperatureValue))
        {
            await DisplayAlert("Validation", "Temperature must be numeric.", "OK");
            return;
        }

        decimal temperature = MeasurementService.ToCelsius(temperatureValue);
        string rainfallUnit = MeasurementService.RainfallUnit;
        string rainDefault = existing is null ? "0" : MeasurementService.FromMillimeters(existing.Rainfall).ToString("0.#");
        string? rainText = await DisplayPromptAsync(title, $"Rainfall ({rainfallUnit})", initialValue: rainDefault, keyboard: Keyboard.Numeric);
        if (!decimal.TryParse(rainText, out decimal rainfallValue) || rainfallValue < 0)
        {
            await DisplayAlert("Validation", "Rainfall must be zero or positive.", "OK");
            return;
        }

        decimal rainfall = MeasurementService.ToMillimeters(rainfallValue);

        string forecastDefault = existing?.Forecast ?? "Clear";
        string? forecast = await DisplayPromptAsync(title, "Forecast/Conditions", initialValue: forecastDefault, maxLength: 200);
        if (string.IsNullOrWhiteSpace(forecast))
        {
            await DisplayAlert("Validation", "Forecast is required.", "OK");
            return;
        }

        bool ok = isEdit
            ? await _apiService.UpdateWeatherLogAsync(existing!.WeatherLogId, region.RegionId, dateRecorded, temperature, rainfall, forecast.Trim())
            : await _apiService.AddWeatherLogAsync(region.RegionId, dateRecorded, temperature, rainfall, forecast.Trim());

        if (!ok)
        {
            await DisplayAlert("Save Failed", _apiService.LastError ?? "Could not save weather log.", "OK");
            return;
        }

        await LoadWeatherAsync();
    }

    private async Task AddRegionAsync()
    {
        string? name = await DisplayPromptAsync("Add Region", "Region name", maxLength: 80);
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        bool ok = await _apiService.AddRegionAsync(name.Trim());
        if (!ok)
        {
            await DisplayAlert("Create Region Failed", _apiService.LastError ?? "Could not create region.", "OK");
            return;
        }

        await EnsureRegionsLoadedAsync();
        await DisplayAlert("Success", "Region added successfully.", "OK");
    }

    private static bool IsCritical(string? forecast)
    {
        if (string.IsNullOrWhiteSpace(forecast))
        {
            return false;
        }

        string value = forecast.ToLowerInvariant();
        return value.Contains("frost") || value.Contains("storm") || value.Contains("heavy rain") || value.Contains("hail");
    }

    private static WeatherLogCard ToLogCard(WeatherLog log)
    {
        return new WeatherLogCard
        {
            WeatherLogId = log.WeatherLogId,
            DateRecorded = log.DateRecorded,
            Temperature = log.Temperature,
            Rainfall = log.Rainfall,
            Forecast = log.Forecast
        };
    }

    private async void OnFetchClicked(object sender, EventArgs e)
    {
        CropRegion? region = RegionPicker.SelectedItem as CropRegion;
        if (region == null)
        {
            await DisplayAlert("Select Region", "Please select a region first.", "OK");
            return;
        }

        bool imported = await _apiService.FetchAndStoreOpenMeteoWeatherAsync(region.RegionId);
        if (!imported)
        {
            await DisplayAlert("Fetch Failed", _apiService.LastError ?? "Could not fetch weather from Open-Meteo.", "OK");
        }

        await LoadWeatherAsync();
    }

    private async void OnAddWeatherLogClicked(object sender, EventArgs e)
    {
        await AddOrEditWeatherAsync(null);
    }

    private async void OnEditLogSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipeItem || swipeItem.CommandParameter is not WeatherLogCard card)
        {
            return;
        }

        WeatherLog? existing = _logs.FirstOrDefault(l => l.WeatherLogId == card.WeatherLogId);
        if (existing is not null)
        {
            await AddOrEditWeatherAsync(existing);
        }
    }

    private async void OnDeleteLogSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipeItem || swipeItem.CommandParameter is not WeatherLogCard card)
        {
            return;
        }

        bool confirm = await DisplayAlert("Delete Weather Log", $"Delete weather log from {card.DateText}?", "Delete", "Cancel");
        if (!confirm)
        {
            return;
        }

        bool ok = await _apiService.DeleteWeatherLogAsync(card.WeatherLogId);
        if (!ok)
        {
            await DisplayAlert("Delete Failed", _apiService.LastError ?? "Could not delete weather log.", "OK");
            return;
        }

        await LoadWeatherAsync();
    }

    private async void OnAddRegionClicked(object sender, EventArgs e)
    {
        await AddRegionAsync();
    }

    private async void OnRefreshRequested(object sender, EventArgs e)
    {
        await LoadWeatherAsync();
    }

    private sealed class WeatherLogCard
    {
        public int WeatherLogId { get; init; }
        public DateTime DateRecorded { get; init; }
        public decimal Temperature { get; init; }
        public decimal Rainfall { get; init; }
        public string Forecast { get; init; } = string.Empty;

        public string DateText => DateRecorded.ToString("dd MMM yyyy");
        public string TemperatureText => MeasurementService.FormatTemperature(Temperature);
        public string RainfallText => MeasurementService.FormatRainfall(Rainfall);
    }
}
