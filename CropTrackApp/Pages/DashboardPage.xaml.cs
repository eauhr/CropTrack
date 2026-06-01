using CropTrackApp.Models;
using CropTrackApp.Services;

namespace CropTrackApp.Pages;

public partial class DashboardPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private bool _isLoading;

    public DashboardPage(ApiService apiService, AuthService authService)
    {
        InitializeComponent();
        _apiService = apiService;
        _authService = authService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        try
        {
            string farmerName = await _authService.GetFarmerNameAsync() ?? "Farmer";
            WelcomeLabel.Text = $"Welcome, {farmerName}";

            int farmerId = await _authService.GetFarmerIdAsync();
            List<Crop> crops = await _apiService.GetCropsAsync();
            List<Field> fields = farmerId > 0 ? await _apiService.GetFieldsAsync(farmerId) : new List<Field>();
            List<CropRegion> regions = await _apiService.GetRegionsAsync();

            List<FieldCrop> plantings = new();
            foreach (Field field in fields)
            {
                try
                {
                    plantings.AddRange(await _apiService.GetFieldCropsAsync(field.FieldId));
                }
                catch
                {
                    // Keep dashboard resilient.
                }
            }

            CropsCountLabel.Text = crops.Count.ToString();
            FieldsCountLabel.Text = fields.Count.ToString();
            AcreageLabel.Text = MeasurementService.FormatArea(fields.Sum(f => f.Acres));
            PlantingsCountLabel.Text = plantings.Count(p => p.HarvestDate.Date >= DateTime.Today).ToString();

            await BindRevenueAndWarningsAsync(crops, fields, plantings, regions);
            BuildFocusLines(crops, fields, plantings);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Dashboard Error", ex.Message, "OK");
        }
        finally
        {
            _isLoading = false;
            DashboardRefreshView.IsRefreshing = false;
        }
    }

    private void BuildFocusLines(List<Crop> crops, List<Field> fields, List<FieldCrop> plantings)
    {
        DateTime today = DateTime.Today;
        int overdue = plantings.Count(p => p.HarvestDate.Date < today);
        int dueSoon = plantings.Count(p => p.HarvestDate.Date >= today && p.HarvestDate.Date <= today.AddDays(3));

        PerformanceLine1Label.Text = overdue > 0
            ? $"- {overdue} overdue harvest(s). Start with harvest logistics."
            : "- No overdue harvests. Good control on timelines.";

        PerformanceLine2Label.Text = dueSoon > 0
            ? $"- {dueSoon} planting(s) reach harvest in 3 days. Prepare crew and storage."
            : "- No urgent harvest deadlines in the next 3 days.";

        if (fields.Count == 0 || crops.Count == 0)
        {
            PerformanceLine3Label.Text = "- Add at least one field and crop to unlock full planning.";
            return;
        }

        int active = plantings.Count(p => p.HarvestDate.Date >= today);
        int finished = plantings.Count - active;
        PerformanceLine3Label.Text = $"- Active cycles: {active}, completed cycles: {Math.Max(0, finished)}.";
    }

    private async Task BindRevenueAndWarningsAsync(
        List<Crop> crops,
        List<Field> fields,
        List<FieldCrop> plantings,
        List<CropRegion> regions)
    {
        Dictionary<int, decimal> latestPriceByCrop = new();
        foreach (int cropId in plantings.Select(p => p.CropId).Distinct())
        {
            try
            {
                List<MarketPrice> prices = await _apiService.GetMarketPricesAsync(cropId);
                MarketPrice? latest = prices.OrderByDescending(p => p.DateRecorded).FirstOrDefault();
                if (latest is not null)
                {
                    latestPriceByCrop[cropId] = latest.PricePerTon;
                }
            }
            catch
            {
                // Keep dashboard resilient even if one crop price fails.
            }
        }

        decimal predictedRevenue = 0m;
        int pricedPlantings = 0;
        foreach (FieldCrop planting in plantings.Where(p => p.HarvestDate.Date >= DateTime.Today))
        {
            if (!latestPriceByCrop.TryGetValue(planting.CropId, out decimal latestPrice))
            {
                continue;
            }

            predictedRevenue += planting.QuantityInTons * latestPrice;
            pricedPlantings++;
        }

        PredictedRevenueLabel.Text = $"{predictedRevenue:0.##}";
        RevenueNoteLabel.Text = pricedPlantings > 0
            ? $"Calculated from {pricedPlantings} active planting(s) with market prices."
            : "No active plantings with price data yet.";

        int warningCount = 0;
        string warningNote = "No critical warnings";
        foreach (CropRegion region in regions)
        {
            try
            {
                List<WeatherLog> logs = await _apiService.GetWeatherLogsAsync(region.RegionId);
                WeatherLog? critical = logs
                    .OrderByDescending(l => l.DateRecorded)
                    .FirstOrDefault(l => IsCriticalForecast(l.Forecast));
                if (critical is not null)
                {
                    warningCount++;
                    warningNote = $"{region.Name}: {critical.Forecast}";
                }
            }
            catch
            {
                // Ignore one region failures.
            }
        }

        WeatherWarningCountLabel.Text = warningCount.ToString();
        WeatherWarningNoteLabel.Text = warningNote;

        DateTime today = DateTime.Today;
        int overdue = plantings.Count(p => p.HarvestDate.Date < today);
        int dueSoon = plantings.Count(p => p.HarvestDate.Date >= today && p.HarvestDate.Date <= today.AddDays(3));
        if (overdue > 0)
        {
            DailyBriefLabel.Text = $"{overdue} harvest(s) overdue. Handle harvest operations first, then update Game Plan.";
        }
        else if (warningCount > 0)
        {
            DailyBriefLabel.Text = $"Weather alert active ({warningCount} region(s)). Review sensitive crops before field work.";
        }
        else if (dueSoon > 0)
        {
            DailyBriefLabel.Text = $"{dueSoon} harvest(s) due within 3 days. Prepare transport, storage, and labor.";
        }
        else
        {
            DailyBriefLabel.Text = "Operations look stable today. Use Game Plan to prepare the next planting cycle.";
        }
    }

    private static bool IsCriticalForecast(string? forecast)
    {
        if (string.IsNullOrWhiteSpace(forecast))
        {
            return false;
        }

        string value = forecast.ToLowerInvariant();
        return value.Contains("frost") || value.Contains("storm") || value.Contains("heavy rain") || value.Contains("hail");
    }

    private async void OnRefreshRequested(object sender, EventArgs e)
    {
        await LoadDashboardAsync();
    }

    private async void OnOpenGamePlanClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//GamePlanPage");
    }

    private async void OnOpenFieldsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//FieldsPage");
    }

    private async void OnOpenProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProfilePage");
    }
}
