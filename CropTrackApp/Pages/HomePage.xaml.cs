using CropTrackApp.Models;
using CropTrackApp.Services;
using Microsoft.Maui.Storage;

namespace CropTrackApp.Pages;

public partial class HomePage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly List<CropRegion> _regions = new();
    private readonly List<WeatherLog> _currentRegionLogs = new();
    private readonly List<TodayTaskItem> _todayTasks = new();
    private readonly List<GameTaskProgressItem> _taskProgress = new();
    private bool _isLoading;

    public HomePage(ApiService apiService, AuthService authService)
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
            WelcomeTitleLabel.Text = $"Welcome, {farmerName}";

            int farmerId = await _authService.GetFarmerIdAsync();
            List<Crop> crops = await _apiService.GetCropsAsync();
            List<Field> fields = farmerId > 0 ? await _apiService.GetFieldsAsync(farmerId) : new List<Field>();

            Dictionary<int, string> cropNames = crops.ToDictionary(c => c.CropId, c => c.Name);
            List<FieldCrop> allPlantings = new();

            foreach (Field field in fields)
            {
                try
                {
                    List<FieldCrop> fieldCrops = await _apiService.GetFieldCropsAsync(field.FieldId);
                    allPlantings.AddRange(fieldCrops);
                }
                catch
                {
                    // Skip one field if plantings fetch fails.
                }
            }

            int activePlantings = allPlantings.Count(p => p.HarvestDate.Date >= DateTime.Today);
            decimal totalAcreage = fields.Sum(f => f.Acres);

            TotalCropsLabel.Text = crops.Count.ToString();
            TotalFieldsLabel.Text = fields.Count.ToString();
            TotalAcreageLabel.Text = MeasurementService.FormatArea(totalAcreage);
            ActivePlantingsLabel.Text = activePlantings.ToString();

            BindRecentActivity(allPlantings, cropNames);
            BindFieldCropPlan(fields, allPlantings, cropNames);
            await LoadRegionsAndWeatherAsync();
            await BindAlertsAsync(allPlantings, cropNames, crops);
            await BuildDailyBriefAsync(crops, fields, allPlantings, cropNames);
            BuildTaskProgress(crops, fields, allPlantings);
            BuildSeasonPlanner(crops, allPlantings, cropNames);
            await BuildActionPlanAsync(crops, fields, allPlantings, cropNames);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Dashboard Error", ex.Message, "OK");
        }
        finally
        {
            DashboardRefreshView.IsRefreshing = false;
            _isLoading = false;
        }
    }

    private void BindRecentActivity(List<FieldCrop> allPlantings, Dictionary<int, string> cropNames)
    {
        List<ActivityItem> activity = allPlantings
            .OrderByDescending(p => p.PlantingDate)
            .Take(8)
            .Select(p => new ActivityItem
            {
                Title = $"Planted {ResolveCropName(cropNames, p.CropId)}",
                Detail = $"Field #{p.FieldId} - Qty {MeasurementService.FormatWeight(p.QuantityInTons)}",
                TimeText = p.PlantingDate.ToString("dd MMM yyyy")
            })
            .ToList();

        ActivityCollectionView.ItemsSource = activity;
    }

    private async Task LoadRegionsAndWeatherAsync()
    {
        _regions.Clear();
        List<CropRegion> regions = await _apiService.GetRegionsAsync();
        _regions.AddRange(regions);

        WeatherRegionPicker.ItemsSource = null;
        WeatherRegionPicker.ItemsSource = _regions;

        if (_regions.Count == 0)
        {
            CurrentWeatherLabel.Text = "Current conditions: no regions configured.";
            TemperatureRangeLabel.Text = "Today's range: N/A";
            ForecastDay1Label.Text = "Day 1";
            ForecastDay2Label.Text = "Day 2";
            ForecastDay3Label.Text = "Day 3";
            CriticalAlertsLabel.Text = "Critical alerts: none";
            return;
        }

        if (WeatherRegionPicker.SelectedIndex < 0)
        {
            WeatherRegionPicker.SelectedIndex = 0;
        }

        await LoadWeatherForSelectedRegionAsync();
    }

    private async Task LoadWeatherForSelectedRegionAsync()
    {
        CropRegion? selectedRegion = WeatherRegionPicker.SelectedItem as CropRegion;
        if (selectedRegion is null)
        {
            return;
        }

        try
        {
            List<WeatherLog> logs = await _apiService.GetWeatherLogsAsync(selectedRegion.RegionId);
            _currentRegionLogs.Clear();
            _currentRegionLogs.AddRange(logs.OrderByDescending(w => w.DateRecorded));

            if (_currentRegionLogs.Count == 0)
            {
                CurrentWeatherLabel.Text = $"Current conditions ({selectedRegion.Name}): no logs yet.";
                TemperatureRangeLabel.Text = "Today's range: N/A";
                ForecastDay1Label.Text = "Day 1";
                ForecastDay2Label.Text = "Day 2";
                ForecastDay3Label.Text = "Day 3";
                CriticalAlertsLabel.Text = "Critical alerts: none";
                return;
            }

            WeatherLog latest = _currentRegionLogs[0];
            CurrentWeatherLabel.Text = $"Current ({selectedRegion.Name}): {latest.Forecast} - {MeasurementService.FormatTemperature(latest.Temperature)}";

            DateTime today = DateTime.Today;
            List<WeatherLog> todayLogs = _currentRegionLogs.Where(w => w.DateRecorded.Date == today).ToList();
            if (todayLogs.Count > 0)
            {
                decimal min = todayLogs.Min(w => w.Temperature);
                decimal max = todayLogs.Max(w => w.Temperature);
                TemperatureRangeLabel.Text = $"Today's range: {MeasurementService.FormatTemperature(min)} to {MeasurementService.FormatTemperature(max)}";
            }
            else
            {
                TemperatureRangeLabel.Text = "Today's range: no logs today.";
            }

            List<WeatherLog> forecast = _currentRegionLogs.OrderBy(w => w.DateRecorded).Take(3).ToList();
            ForecastDay1Label.Text = forecast.Count > 0 ? $"{forecast[0].DateRecorded:dd MMM}\n{MeasurementService.FormatTemperature(forecast[0].Temperature)}" : "Day 1";
            ForecastDay2Label.Text = forecast.Count > 1 ? $"{forecast[1].DateRecorded:dd MMM}\n{MeasurementService.FormatTemperature(forecast[1].Temperature)}" : "Day 2";
            ForecastDay3Label.Text = forecast.Count > 2 ? $"{forecast[2].DateRecorded:dd MMM}\n{MeasurementService.FormatTemperature(forecast[2].Temperature)}" : "Day 3";

            string? critical = _currentRegionLogs.Select(l => l.Forecast).FirstOrDefault(IsCriticalForecast);
            CriticalAlertsLabel.Text = critical is null ? "Critical alerts: none" : $"Critical alerts: {critical}";
        }
        catch (Exception ex)
        {
            CurrentWeatherLabel.Text = $"Current conditions: error loading weather ({ex.Message})";
        }
    }

    private async Task BindAlertsAsync(List<FieldCrop> allPlantings, Dictionary<int, string> cropNames, List<Crop> crops)
    {
        List<AlertItem> alerts = new();
        DateTime today = DateTime.Today;

        foreach (FieldCrop planting in allPlantings.Where(p => p.HarvestDate.Date <= today.AddDays(3)).OrderBy(p => p.HarvestDate).Take(5))
        {
            string tone = planting.HarvestDate.Date < today ? "Overdue" : "Due soon";
            alerts.Add(new AlertItem
            {
                Title = $"Harvest reminder: {ResolveCropName(cropNames, planting.CropId)}",
                Detail = $"{tone} - Field #{planting.FieldId} - Harvest {planting.HarvestDate:dd MMM yyyy}"
            });
        }

        WeatherLog? critical = _currentRegionLogs.FirstOrDefault(w => IsCriticalForecast(w.Forecast));
        if (critical is not null)
        {
            alerts.Add(new AlertItem
            {
                Title = "Weather alert",
                Detail = $"{critical.Forecast} - {critical.DateRecorded:dd MMM yyyy}"
            });
        }

        if (crops.Count > 0)
        {
            int sampleCropId = crops[0].CropId;
            try
            {
                List<MarketPrice> prices = await _apiService.GetMarketPricesAsync(sampleCropId);
                if (prices.Count >= 2)
                {
                    List<MarketPrice> ordered = prices.OrderByDescending(p => p.DateRecorded).Take(2).ToList();
                    decimal diff = ordered[0].PricePerTon - ordered[1].PricePerTon;
                    if (Math.Abs(diff) >= 1m)
                    {
                        string direction = diff > 0 ? "up" : "down";
                        alerts.Add(new AlertItem
                        {
                            Title = "Price update",
                            Detail = $"{crops[0].Name} is {direction} by {MeasurementService.PricePerPreferredWeight(Math.Abs(diff)):0.##} per {MeasurementService.WeightUnit}"
                        });
                    }
                }
            }
            catch
            {
                // Ignore pricing alert failure.
            }
        }

        AlertsCollectionView.ItemsSource = alerts;
    }

    private async Task BuildActionPlanAsync(
        List<Crop> crops,
        List<Field> fields,
        List<FieldCrop> allPlantings,
        Dictionary<int, string> cropNames)
    {
        _todayTasks.Clear();
        DateTime today = DateTime.Today;

        if (fields.Count == 0)
        {
            _todayTasks.Add(new TodayTaskItem
            {
                Title = "Create your first field",
                Detail = "Fields unlock plantings, weather targeting, and acreage tracking.",
                ActionText = "Add field",
                ActionKey = "add_field"
            });
        }

        if (crops.Count == 0)
        {
            _todayTasks.Add(new TodayTaskItem
            {
                Title = "Add your crop catalog",
                Detail = "Set crop types to track growth windows and expected yield.",
                ActionText = "Add crop",
                ActionKey = "add_crop"
            });
        }

        if (_regions.Count == 0)
        {
            _todayTasks.Add(new TodayTaskItem
            {
                Title = "Enable local weather alerts",
                Detail = "Add at least one region to start risk monitoring.",
                ActionText = "Weather",
                ActionKey = "open_weather"
            });
        }

        FieldCrop? overdueHarvest = allPlantings
            .Where(p => p.HarvestDate.Date < today)
            .OrderBy(p => p.HarvestDate)
            .FirstOrDefault();
        if (overdueHarvest is not null)
        {
            _todayTasks.Add(new TodayTaskItem
            {
                Title = $"Harvest overdue: {ResolveCropName(cropNames, overdueHarvest.CropId)}",
                Detail = $"Field #{overdueHarvest.FieldId} was due on {overdueHarvest.HarvestDate:dd MMM}.",
                ActionText = "Review",
                ActionKey = "open_fields"
            });
        }

        FieldCrop? dueSoon = allPlantings
            .Where(p => p.HarvestDate.Date >= today && p.HarvestDate.Date <= today.AddDays(3))
            .OrderBy(p => p.HarvestDate)
            .FirstOrDefault();
        if (dueSoon is not null)
        {
            int daysLeft = Math.Max(0, (dueSoon.HarvestDate.Date - today).Days);
            _todayTasks.Add(new TodayTaskItem
            {
                Title = $"Harvest prep: {ResolveCropName(cropNames, dueSoon.CropId)}",
                Detail = $"Field #{dueSoon.FieldId} reaches harvest in {daysLeft} day(s).",
                ActionText = "Plan",
                ActionKey = "open_fields"
            });
        }

        WeatherLog? criticalWeather = _currentRegionLogs.FirstOrDefault(w => IsCriticalForecast(w.Forecast));
        if (criticalWeather is not null)
        {
            _todayTasks.Add(new TodayTaskItem
            {
                Title = "Weather risk detected",
                Detail = $"{criticalWeather.Forecast}. Consider protection actions today.",
                ActionText = "Fetch",
                ActionKey = "fetch_weather"
            });
        }

        if (crops.Count > 0)
        {
            try
            {
                Crop sampleCrop = crops[0];
                List<MarketPrice> prices = await _apiService.GetMarketPricesAsync(sampleCrop.CropId);
                List<MarketPrice> ordered = prices.OrderByDescending(p => p.DateRecorded).Take(2).ToList();
                if (ordered.Count == 0)
                {
                    _todayTasks.Add(new TodayTaskItem
                    {
                        Title = "No market price data",
                        Detail = $"Fetch USDA price for {sampleCrop.Name} before selling.",
                        ActionText = "Prices",
                        ActionKey = "open_prices"
                    });
                }
                else if (ordered.Count >= 2)
                {
                    decimal delta = ordered[0].PricePerTon - ordered[1].PricePerTon;
                    if (delta >= 1m)
                    {
                        _todayTasks.Add(new TodayTaskItem
                        {
                            Title = "Potential sell window",
                            Detail = $"{sampleCrop.Name} moved up by {delta:0.##} since last update.",
                            ActionText = "Prices",
                            ActionKey = "open_prices"
                        });
                    }
                }
            }
            catch
            {
                // Skip recommendation if market fetch fails.
            }
        }

        TodayPlanCollectionView.ItemsSource = _todayTasks.Take(5).ToList();
        PlanSummaryLabel.Text = _todayTasks.Count == 0
            ? "No urgent tasks. Focus on routine monitoring."
            : $"{_todayTasks.Count} actionable item(s) identified.";

        DecisionInsightLabel.Text = BuildDecisionInsight(crops, allPlantings);
    }

    private async Task BuildDailyBriefAsync(
        List<Crop> crops,
        List<Field> fields,
        List<FieldCrop> allPlantings,
        Dictionary<int, string> cropNames)
    {
        DateTime today = DateTime.Today;
        int overdueCount = allPlantings.Count(p => p.HarvestDate.Date < today);
        int dueSoonCount = allPlantings.Count(p => p.HarvestDate.Date >= today && p.HarvestDate.Date <= today.AddDays(3));
        bool hasCriticalWeather = _currentRegionLogs.Any(l => IsCriticalForecast(l.Forecast));

        List<PriceSnapshot> priceSnapshots = await LoadPriceSnapshotsAsync(crops, allPlantings);
        bool hasAnyPrice = priceSnapshots.Any(p => p.HasData);
        PriceSnapshot? strongestPositiveMove = priceSnapshots
            .Where(p => p.ChangePerTon > 0)
            .OrderByDescending(p => p.ChangePerTon)
            .FirstOrDefault();

        int setupCompleted = 0;
        setupCompleted += crops.Count > 0 ? 1 : 0;
        setupCompleted += fields.Count > 0 ? 1 : 0;
        setupCompleted += _regions.Count > 0 ? 1 : 0;
        setupCompleted += allPlantings.Count > 0 ? 1 : 0;
        setupCompleted += _currentRegionLogs.Count > 0 ? 1 : 0;
        setupCompleted += hasAnyPrice ? 1 : 0;

        int setupScore = (int)Math.Round((setupCompleted / 6d) * 100d);
        int riskScore = Math.Min(100, overdueCount * 25 + dueSoonCount * 10 + (hasCriticalWeather ? 30 : 0));
        int opportunityScore = strongestPositiveMove is null
            ? (hasAnyPrice ? 35 : 15)
            : Math.Min(100, 55 + (int)Math.Round((double)strongestPositiveMove.ChangePerTon * 8d));

        SetupScoreLabel.Text = $"{setupScore}%";
        RiskScoreLabel.Text = $"{riskScore}/100";
        OpportunityScoreLabel.Text = $"{opportunityScore}/100";

        if (setupScore < 50)
        {
            DailyBriefLabel.Text = "Priority: finish setup (crops, fields, region, and first planting) so the app can guide decisions with real context.";
            return;
        }

        if (riskScore >= 70)
        {
            DailyBriefLabel.Text = $"Urgent: {overdueCount} overdue harvest(s) and {dueSoonCount} due soon. Focus labor and harvest logistics first.";
            return;
        }

        if (hasCriticalWeather)
        {
            string forecast = _currentRegionLogs.First(l => IsCriticalForecast(l.Forecast)).Forecast;
            DailyBriefLabel.Text = $"Weather risk: {forecast}. Protect sensitive crops, then revisit planting schedule.";
            return;
        }

        if (strongestPositiveMove is not null)
        {
            string cropName = ResolveCropName(cropNames, strongestPositiveMove.CropId);
            DailyBriefLabel.Text = $"Market signal: {cropName} is up by {strongestPositiveMove.ChangePerTon:0.##}/ton. Review inventory and plan potential sales.";
            return;
        }

        DailyBriefLabel.Text = "Stable day: no major risks detected. Refresh weather and prices, then prepare next planting cycle.";
    }

    private async Task<List<PriceSnapshot>> LoadPriceSnapshotsAsync(List<Crop> crops, List<FieldCrop> allPlantings)
    {
        List<PriceSnapshot> snapshots = new();
        HashSet<int> cropIds = allPlantings.Select(p => p.CropId).ToHashSet();
        if (cropIds.Count == 0)
        {
            cropIds = crops.Select(c => c.CropId).Take(3).ToHashSet();
        }

        foreach (int cropId in cropIds.Take(6))
        {
            try
            {
                List<MarketPrice> prices = await _apiService.GetMarketPricesAsync(cropId);
                List<MarketPrice> ordered = prices.OrderByDescending(p => p.DateRecorded).Take(2).ToList();
                if (ordered.Count == 0)
                {
                    snapshots.Add(new PriceSnapshot { CropId = cropId, HasData = false, ChangePerTon = 0m });
                    continue;
                }

                decimal change = ordered.Count >= 2 ? ordered[0].PricePerTon - ordered[1].PricePerTon : 0m;
                snapshots.Add(new PriceSnapshot { CropId = cropId, HasData = true, ChangePerTon = change });
            }
            catch
            {
                snapshots.Add(new PriceSnapshot { CropId = cropId, HasData = false, ChangePerTon = 0m });
            }
        }

        return snapshots;
    }

    private void BindFieldCropPlan(List<Field> fields, List<FieldCrop> allPlantings, Dictionary<int, string> cropNames)
    {
        List<FieldCropPlanItem> planItems = allPlantings
            .OrderBy(p => p.HarvestDate)
            .Take(12)
            .Select(p =>
            {
                Field? field = fields.FirstOrDefault(f => f.FieldId == p.FieldId);
                string cropName = ResolveCropName(cropNames, p.CropId);
                int days = (p.HarvestDate.Date - DateTime.Today).Days;
                string status = days < 0
                    ? $"Overdue by {Math.Abs(days)} day(s)"
                    : days == 0
                        ? "Harvest today"
                        : $"Harvest in {days} day(s)";

                return new FieldCropPlanItem
                {
                    Title = $"{field?.Name ?? $"Field #{p.FieldId}"} - {cropName}",
                    Detail = $"Planted {p.PlantingDate:dd MMM yyyy} | Qty {MeasurementService.FormatWeight(p.QuantityInTons)}",
                    Status = status
                };
            })
            .ToList();

        FieldCropPlanCollectionView.ItemsSource = planItems;
        PlanCoverageLabel.Text = planItems.Count == 0
            ? "No crop plans yet."
            : $"{planItems.Count} active/queued field crop plan item(s).";
    }

    private void BuildTaskProgress(List<Crop> crops, List<Field> fields, List<FieldCrop> allPlantings)
    {
        _taskProgress.Clear();

        List<GameTaskProgressItem> defaults = new()
        {
            NewTask("setup_field", "Field setup", fields.Count > 0 ? "Field base is ready." : "Add at least one field."),
            NewTask("setup_crop", "Crop catalog", crops.Count > 0 ? "Crop catalog available." : "Add at least one crop."),
            NewTask("setup_planting", "First planting", allPlantings.Count > 0 ? "Plantings are tracked." : "Create first planting in a field."),
            NewTask("review_weather", "Weather review", _currentRegionLogs.Count > 0 ? "Weather logs available." : "Fetch weather logs for your region."),
            NewTask("review_prices", "Price review", "Check crop price trend before sale decisions.")
        };

        foreach (GameTaskProgressItem task in defaults)
        {
            bool completed = Preferences.Get($"gameplan_task_{task.Key}", false);
            _taskProgress.Add(new GameTaskProgressItem
            {
                Key = task.Key,
                Title = task.Title,
                Detail = task.Detail,
                IsDone = completed
            });
        }

        TaskProgressCollectionView.ItemsSource = null;
        TaskProgressCollectionView.ItemsSource = _taskProgress;
        BindTaskProgressSummary();
    }

    private GameTaskProgressItem NewTask(string key, string title, string detail)
    {
        return new GameTaskProgressItem
        {
            Key = key,
            Title = title,
            Detail = detail,
            IsDone = false
        };
    }

    private void BindTaskProgressSummary()
    {
        int done = _taskProgress.Count(t => t.IsDone);
        int total = _taskProgress.Count;
        int todo = Math.Max(0, total - done);
        TaskProgressSummaryLabel.Text = $"{done} done | {todo} to do";
    }

    private void BuildSeasonPlanner(List<Crop> crops, List<FieldCrop> allPlantings, Dictionary<int, string> cropNames)
    {
        DateTime today = DateTime.Today;
        List<PlantingWindowItem> windows = BuildPlantingWindows(crops, allPlantings);
        List<HarvestTimelineItem> harvestTimeline = allPlantings
            .Where(p => p.HarvestDate.Date >= today && p.HarvestDate.Date <= today.AddDays(30))
            .OrderBy(p => p.HarvestDate)
            .Take(8)
            .Select(p =>
            {
                int daysLeft = (p.HarvestDate.Date - today).Days;
                string urgency = daysLeft <= 3 ? "Urgent" : daysLeft <= 10 ? "Soon" : "Planned";
                return new HarvestTimelineItem
                {
                    Title = $"{ResolveCropName(cropNames, p.CropId)} - Field #{p.FieldId}",
                    Detail = $"{urgency}: harvest on {p.HarvestDate:dd MMM yyyy} ({Math.Max(0, daysLeft)} day(s))"
                };
            })
            .ToList();

        List<WorkloadForecastItem> workload = BuildWorkloadForecast(windows, allPlantings);

        PlantingWindowsCollectionView.ItemsSource = windows;
        HarvestTimelineCollectionView.ItemsSource = harvestTimeline;
        WorkloadForecastCollectionView.ItemsSource = workload;

        int highPriorityWindows = windows.Count(w => w.IsHighPriority);
        int harvestCount = harvestTimeline.Count;
        SeasonWindowSummaryLabel.Text = highPriorityWindows > 0
            ? $"{highPriorityWindows} high-priority planting window(s) and {harvestCount} harvest action(s) in the next 30 days."
            : $"{windows.Count} planting window(s) mapped and {harvestCount} harvest action(s) in the next 30 days.";
    }

    private static List<PlantingWindowItem> BuildPlantingWindows(List<Crop> crops, List<FieldCrop> allPlantings)
    {
        DateTime today = DateTime.Today;
        List<PlantingWindowItem> windows = new();

        foreach (Crop crop in crops.Take(8))
        {
            List<FieldCrop> history = allPlantings
                .Where(p => p.CropId == crop.CropId)
                .OrderByDescending(p => p.PlantingDate)
                .ToList();

            DateTime suggestedPlanting;
            string guidance;
            bool highPriority;

            if (history.Count == 0)
            {
                suggestedPlanting = today.AddDays(1);
                guidance = "No planting history yet. Start a pilot planting this week.";
                highPriority = true;
            }
            else
            {
                FieldCrop latest = history[0];
                if (latest.HarvestDate.Date < today)
                {
                    suggestedPlanting = today.AddDays(1);
                    guidance = "Last cycle completed. Replant to keep field utilization high.";
                    highPriority = true;
                }
                else
                {
                    suggestedPlanting = latest.HarvestDate.Date.AddDays(3);
                    guidance = "Current cycle active. Prepare input stock and labor for next turnover.";
                    highPriority = false;
                }
            }

            int growthDays = Math.Max(1, crop.AvgGrowthDays);
            DateTime expectedHarvest = suggestedPlanting.AddDays(growthDays);
            string windowText = $"{suggestedPlanting:dd MMM} - {(suggestedPlanting.AddDays(6)):dd MMM}";

            windows.Add(new PlantingWindowItem
            {
                CropId = crop.CropId,
                CropName = crop.Name,
                StartDate = suggestedPlanting.Date,
                WindowText = $"Planting window: {windowText}",
                HarvestText = $"Expected harvest: {expectedHarvest:dd MMM yyyy} (about {growthDays} days)",
                Guidance = guidance,
                IsHighPriority = highPriority
            });
        }

        return windows
            .OrderByDescending(w => w.IsHighPriority)
            .ThenBy(w => w.CropName)
            .ToList();
    }

    private static List<WorkloadForecastItem> BuildWorkloadForecast(List<PlantingWindowItem> windows, List<FieldCrop> allPlantings)
    {
        DateTime today = DateTime.Today;
        List<WorkloadForecastItem> weeks = new();

        for (int i = 0; i < 3; i++)
        {
            DateTime weekStart = today.AddDays(i * 7);
            DateTime weekEnd = weekStart.AddDays(6);

            int harvestTasks = allPlantings.Count(p => p.HarvestDate.Date >= weekStart && p.HarvestDate.Date <= weekEnd);
            int plantingTasks = windows.Count(w => w.StartDate >= weekStart && w.StartDate <= weekEnd);
            int totalTasks = harvestTasks + plantingTasks;

            string hint = totalTasks switch
            {
                >= 8 => "Heavy week: lock labor and transport early.",
                >= 4 => "Moderate week: align crew and equipment by mid-week.",
                _ => "Light week: ideal time for maintenance and planning."
            };

            weeks.Add(new WorkloadForecastItem
            {
                WeekLabel = $"{weekStart:dd MMM} - {weekEnd:dd MMM}",
                TasksText = $"Planting tasks: {plantingTasks} | Harvest tasks: {harvestTasks}",
                CapacityHint = hint,
                TaskLoad = totalTasks
            });
        }

        return weeks;
    }

    private string BuildDecisionInsight(List<Crop> crops, List<FieldCrop> allPlantings)
    {
        DateTime today = DateTime.Today;
        int dueThisWeek = allPlantings.Count(p => p.HarvestDate.Date >= today && p.HarvestDate.Date <= today.AddDays(7));

        if (crops.Count == 0 || allPlantings.Count == 0)
        {
            return "Start with one crop, one field, and one planting to unlock practical guidance.";
        }

        if (_currentRegionLogs.Any() && IsCriticalForecast(_currentRegionLogs[0].Forecast))
        {
            return "Priority order: weather protection first, then planting and harvest updates.";
        }

        if (dueThisWeek > 0)
        {
            return $"You have {dueThisWeek} planting(s) due this week. Prepare labor, storage, and transport now.";
        }

        return "Operations are stable. Next best move: refresh weather and market prices before planning next plantings.";
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

    private static string ResolveCropName(Dictionary<int, string> cropNames, int cropId)
    {
        return cropNames.TryGetValue(cropId, out string? name) ? name : $"Crop #{cropId}";
    }

    private async void OnRefreshRequested(object sender, EventArgs e)
    {
        await LoadDashboardAsync();
    }

    private async void OnFetchWeatherClicked(object sender, EventArgs e)
    {
        if (WeatherRegionPicker.SelectedItem is CropRegion region)
        {
            bool ok = await _apiService.FetchAndStoreOpenMeteoWeatherAsync(region.RegionId);
            if (!ok && !string.IsNullOrWhiteSpace(_apiService.LastError))
            {
                await DisplayAlert("Fetch Weather", _apiService.LastError, "OK");
            }
        }

        await LoadWeatherForSelectedRegionAsync();
    }

    private async void OnWeatherRegionChanged(object sender, EventArgs e)
    {
        await LoadWeatherForSelectedRegionAsync();
    }

    private async void OnAddCropClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//FieldsPage");
        await DisplayAlert("Crops", "Open the Crops tab inside Fields and Crops page.", "OK");
    }

    private async void OnAddFieldClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//FieldsPage");
    }

    private async void OnAddPlantingClicked(object sender, EventArgs e)
    {
        bool open = await DisplayAlert("Add Planting", "Use Fields page to add a field crop planting.", "Open Fields", "Cancel");
        if (open)
        {
            await Shell.Current.GoToAsync("//FieldsPage");
        }
    }

    private async void OnViewPricesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//DashboardPage");
    }

    private async void OnTaskActionClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not string actionKey)
        {
            return;
        }

        switch (actionKey)
        {
            case "add_field":
            case "open_fields":
                await Shell.Current.GoToAsync("//FieldsPage");
                break;
            case "add_crop":
                await Shell.Current.GoToAsync("//FieldsPage");
                break;
            case "open_weather":
                await Shell.Current.GoToAsync("//DashboardPage");
                break;
            case "open_prices":
                await Shell.Current.GoToAsync("//DashboardPage");
                break;
            case "fetch_weather":
                await OnFetchWeatherFromTaskAsync();
                break;
            default:
                break;
        }
    }

    private async Task OnFetchWeatherFromTaskAsync()
    {
        if (WeatherRegionPicker.SelectedItem is not CropRegion region)
        {
            await DisplayAlert("Weather", "Add/select a region in Game Plan weather widget first.", "OK");
            return;
        }

        bool ok = await _apiService.FetchAndStoreOpenMeteoWeatherAsync(region.RegionId);
        if (!ok)
        {
            await DisplayAlert("Fetch Weather", _apiService.LastError ?? "Could not fetch weather.", "OK");
        }

        await LoadWeatherForSelectedRegionAsync();
    }

    private async void OnRefreshBriefClicked(object sender, EventArgs e)
    {
        await LoadDashboardAsync();
    }

    private void OnTaskDoneChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox checkBox || checkBox.BindingContext is not GameTaskProgressItem item)
        {
            return;
        }

        int idx = _taskProgress.FindIndex(t => t.Key == item.Key);
        if (idx < 0)
        {
            return;
        }

        _taskProgress[idx].IsDone = e.Value;
        Preferences.Set($"gameplan_task_{item.Key}", e.Value);
        BindTaskProgressSummary();
    }

    private sealed class AlertItem
    {
        public string Title { get; init; } = string.Empty;
        public string Detail { get; init; } = string.Empty;
    }

    private sealed class ActivityItem
    {
        public string Title { get; init; } = string.Empty;
        public string Detail { get; init; } = string.Empty;
        public string TimeText { get; init; } = string.Empty;
    }

    private sealed class TodayTaskItem
    {
        public string Title { get; init; } = string.Empty;
        public string Detail { get; init; } = string.Empty;
        public string ActionText { get; init; } = string.Empty;
        public string ActionKey { get; init; } = string.Empty;
    }

    private sealed class PriceSnapshot
    {
        public int CropId { get; init; }
        public bool HasData { get; init; }
        public decimal ChangePerTon { get; init; }
    }

    private sealed class PlantingWindowItem
    {
        public int CropId { get; init; }
        public string CropName { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public string WindowText { get; init; } = string.Empty;
        public string HarvestText { get; init; } = string.Empty;
        public string Guidance { get; init; } = string.Empty;
        public bool IsHighPriority { get; init; }
    }

    private sealed class HarvestTimelineItem
    {
        public string Title { get; init; } = string.Empty;
        public string Detail { get; init; } = string.Empty;
    }

    private sealed class WorkloadForecastItem
    {
        public string WeekLabel { get; init; } = string.Empty;
        public string TasksText { get; init; } = string.Empty;
        public string CapacityHint { get; init; } = string.Empty;
        public int TaskLoad { get; init; }
    }

    private sealed class FieldCropPlanItem
    {
        public string Title { get; init; } = string.Empty;
        public string Detail { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
    }

    private sealed class GameTaskProgressItem
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public bool IsDone { get; set; }
    }
}
