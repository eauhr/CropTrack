using System.Globalization;
using CropTrackApp.Models;
using CropTrackApp.Services;

namespace CropTrackApp.Pages;

public partial class FieldsPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly List<Field> _allFields = new();
    private readonly List<Crop> _allCrops = new();
    private readonly List<CropRegion> _regions = new();
    private readonly List<ProduceKnowledge> _produceCatalog = new();
    private readonly Dictionary<int, int> _fieldPlantings = new();
    private readonly Dictionary<int, string> _fieldCropsPreview = new();
    private bool _isLoadingFields;
    private bool _isLoadingCrops;

    public FieldsPage(ApiService apiService, AuthService authService)
    {
        InitializeComponent();
        _apiService = apiService;
        _authService = authService;
        SortPicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAllAsync();
    }

    private async Task LoadAllAsync()
    {
        await LoadCropsCacheAsync();
        await LoadFieldsAsync();
    }

    private async Task LoadCropsCacheAsync()
    {
        if (_isLoadingCrops)
        {
            return;
        }

        _isLoadingCrops = true;
        try
        {
            List<Crop> crops = await _apiService.GetCropsAsync();
            _allCrops.Clear();
            _allCrops.AddRange(crops);
        }
        catch
        {
            // We can still render fields if crops fail to load.
        }
        finally
        {
            _isLoadingCrops = false;
        }
    }

    private async Task LoadFieldsAsync()
    {
        if (_isLoadingFields)
        {
            return;
        }

        _isLoadingFields = true;
        try
        {
            int farmerId = await _authService.GetFarmerIdAsync();
            List<Field> fields = farmerId > 0
                ? await _apiService.GetFieldsAsync(farmerId)
                : new List<Field>();

            _allFields.Clear();
            _allFields.AddRange(fields);

            _regions.Clear();
            _regions.AddRange(await _apiService.GetRegionsAsync());
            BindRegionFilter();

            _fieldPlantings.Clear();
            _fieldCropsPreview.Clear();
            foreach (Field field in _allFields)
            {
                try
                {
                    List<FieldCrop> plantings = await _apiService.GetFieldCropsAsync(field.FieldId);
                    _fieldPlantings[field.FieldId] = plantings.Count;
                    _fieldCropsPreview[field.FieldId] = BuildCropsPreview(plantings);
                }
                catch
                {
                    _fieldPlantings[field.FieldId] = 0;
                    _fieldCropsPreview[field.FieldId] = "No crops assigned yet.";
                }
            }

            ApplyFieldFilters();
            BindMapSnapshot();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fields Error", ex.Message, "OK");
        }
        finally
        {
            _isLoadingFields = false;
            FieldsRefreshView.IsRefreshing = false;
        }
    }

    private static string BuildCropsPreview(List<FieldCrop> plantings)
    {
        if (plantings.Count == 0)
        {
            return "No crops assigned yet.";
        }

        IEnumerable<string> items = plantings
            .Take(3)
            .Select(p => $"{p.CropName} ({MeasurementService.FormatWeight(p.QuantityInTons)})");

        string preview = $"Crops: {string.Join(", ", items)}";
        if (plantings.Count > 3)
        {
            preview += $" +{plantings.Count - 3} more";
        }

        return preview;
    }

    private void BindRegionFilter()
    {
        List<string> regionOptions = new() { "All Regions" };
        regionOptions.AddRange(_regions.Select(r => r.Name).OrderBy(n => n));

        string? currentSelection = RegionFilterPicker.SelectedItem?.ToString();
        RegionFilterPicker.ItemsSource = regionOptions;

        if (!string.IsNullOrWhiteSpace(currentSelection))
        {
            int index = regionOptions.FindIndex(r => r.Equals(currentSelection, StringComparison.OrdinalIgnoreCase));
            RegionFilterPicker.SelectedIndex = index >= 0 ? index : 0;
        }
        else
        {
            RegionFilterPicker.SelectedIndex = 0;
        }
    }

    private void ApplyFieldFilters()
    {
        IEnumerable<Field> query = _allFields;

        string search = FieldSearchBar.Text?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(f =>
                f.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                f.Location.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        string regionFilter = RegionFilterPicker.SelectedItem?.ToString() ?? "All Regions";
        if (!string.Equals(regionFilter, "All Regions", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(f => string.Equals(ResolveRegionName(f.RegionId), regionFilter, StringComparison.OrdinalIgnoreCase));
        }

        string sort = SortPicker.SelectedItem?.ToString() ?? "Alphabetical";
        query = sort switch
        {
            "Largest acreage" => query.OrderByDescending(f => f.Acres),
            "Smallest acreage" => query.OrderBy(f => f.Acres),
            _ => query.OrderBy(f => f.Name)
        };

        List<FieldCardItem> cards = query.Select(field => new FieldCardItem
        {
            FieldId = field.FieldId,
            Name = field.Name,
            Location = field.Location,
            Acres = field.Acres,
            RegionId = field.RegionId,
            RegionName = ResolveRegionName(field.RegionId),
            PlantingsCount = _fieldPlantings.GetValueOrDefault(field.FieldId),
            CropsPreview = _fieldCropsPreview.GetValueOrDefault(field.FieldId, "No crops assigned yet.")
        }).ToList();

        FieldsCollectionView.ItemsSource = cards;
    }

    private void BindMapSnapshot()
    {
        if (_allFields.Count == 0)
        {
            MapSummaryLabel.Text = "No fields yet. Add your first field to start planning.";
            RegionSpreadLabel.Text = "Region spread: none";
            return;
        }

        decimal totalAcreage = _allFields.Sum(f => f.Acres);
        int activeFields = _allFields.Count(f => _fieldPlantings.GetValueOrDefault(f.FieldId) > 0);
        int idleFields = _allFields.Count - activeFields;

        MapSummaryLabel.Text = $"Total acreage: {MeasurementService.FormatArea(totalAcreage)} across {_allFields.Count} fields. Active: {activeFields}, Idle: {idleFields}.";

        var byRegion = _allFields
            .GroupBy(f => ResolveRegionName(f.RegionId))
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => $"{g.Key} ({g.Count()})");

        RegionSpreadLabel.Text = $"Top regions: {string.Join(", ", byRegion)}";
    }

    private async Task AddOrEditFieldAsync(FieldCardItem? existing)
    {
        bool isEdit = existing is not null;
        string title = isEdit ? "Edit Field" : "Add Field";

        string? name = await DisplayPromptAsync(title, "Field name", initialValue: existing?.Name, maxLength: 100);
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        string? locationInput = await DisplayPromptAsync(title, "Location/address", initialValue: existing?.Location, maxLength: 200);
        if (string.IsNullOrWhiteSpace(locationInput))
        {
            return;
        }

        string location = await ChooseLocationFromApiAsync(locationInput.Trim());

        string areaUnit = MeasurementService.AreaUnit;
        string? acresText = await DisplayPromptAsync(
            title,
            $"Acreage ({areaUnit})",
            initialValue: existing is null ? "1" : MeasurementService.FromAcres(existing.Acres).ToString("0.##"),
            keyboard: Keyboard.Numeric);
        if (!TryParseDecimalInput(acresText, out decimal areaValue) || areaValue <= 0)
        {
            await DisplayAlert("Validation", $"{areaUnit} must be a positive number.", "OK");
            return;
        }

        decimal acres = MeasurementService.ToAcres(areaValue);

        bool ok = isEdit
            ? await _apiService.UpdateFieldAsync(existing!.FieldId, name.Trim(), location, acres)
            : await _apiService.AddFieldAsync(name.Trim(), location, acres);

        if (!ok)
        {
            await DisplayAlert("Save Failed", _apiService.LastError ?? "Could not save field.", "OK");
            return;
        }

        await LoadFieldsAsync();

        FieldCardItem? savedField = ResolveSavedField(existing, name.Trim(), location);
        if (savedField is null)
        {
            await DisplayAlert("Saved", "Field saved successfully.", "OK");
            return;
        }

        bool openCropManager = await DisplayAlert(
            "Field Saved",
            "Do you want to assign crops to this field now?",
            "Yes",
            "Later");

        if (openCropManager)
        {
            await ManageFieldCropsAsync(savedField);
        }
    }

    private FieldCardItem? ResolveSavedField(FieldCardItem? existing, string name, string location)
    {
        if (existing is not null)
        {
            Field? updated = _allFields.FirstOrDefault(f => f.FieldId == existing.FieldId);
            if (updated is null)
            {
                return null;
            }

            return ToCard(updated);
        }

        Field? created = _allFields
            .Where(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                        f.Location.Equals(location, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(f => f.FieldId)
            .FirstOrDefault()
            ?? _allFields.OrderByDescending(f => f.FieldId).FirstOrDefault();

        return created is null ? null : ToCard(created);
    }

    private async Task<string> ChooseLocationFromApiAsync(string locationInput)
    {
        List<LocationSuggestion> suggestions = await _apiService.SearchLocationSuggestionsAsync(locationInput);
        if (suggestions.Count == 0)
        {
            return locationInput;
        }

        string useTyped = $"Use typed: {locationInput}";
        string[] options = suggestions
            .Select(s => s.DisplayName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .Concat(new[] { useTyped })
            .ToArray();

        string? selected = await DisplayActionSheet("Pick location", "Cancel", null, options);
        if (string.IsNullOrWhiteSpace(selected) || selected == "Cancel")
        {
            return locationInput;
        }

        return selected == useTyped ? locationInput : selected;
    }

    private async Task ManageFieldCropsAsync(FieldCardItem field)
    {
        while (true)
        {
            List<FieldCrop> plantings;
            try
            {
                plantings = await _apiService.GetFieldCropsAsync(field.FieldId);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Crops Error", ex.Message, "OK");
                return;
            }

            string summary = plantings.Count == 0
                ? "No crops assigned yet."
                : string.Join(", ", plantings.Take(3).Select(p => $"{p.CropName} ({MeasurementService.FormatWeight(p.QuantityInTons)})"));

            string action = await DisplayActionSheet(
                $"{field.Name}\n{summary}",
                "Done",
                null,
                "Add crop to field",
                "Remove crop from field");

            if (action == "Done" || string.IsNullOrWhiteSpace(action))
            {
                await LoadFieldsAsync();
                return;
            }

            if (action == "Add crop to field")
            {
                await AddCropToFieldAsync(field);
                continue;
            }

            if (action == "Remove crop from field")
            {
                await RemoveCropFromFieldAsync(plantings);
            }
        }
    }

    private async Task AddCropToFieldAsync(FieldCardItem field)
    {
        List<ProduceKnowledge> produceCatalog;
        try
        {
            produceCatalog = await EnsureProduceCatalogAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Crops Error", $"Could not load crop catalog: {ex.Message}", "OK");
            return;
        }

        if (produceCatalog.Count == 0)
        {
            await DisplayAlert("No Crop Data", "No crops were found in the internal crop knowledge base.", "OK");
            return;
        }

        List<ProduceRecommendation> ordered = await BuildProduceRecommendationsAsync(produceCatalog);
        string[] options = ordered.Select(o => o.DisplayLabel).ToArray();
        string? selectedOption = await DisplayActionSheet("Choose crop (recommended first)", "Cancel", null, options);
        if (string.IsNullOrWhiteSpace(selectedOption) || selectedOption == "Cancel")
        {
            return;
        }

        ProduceRecommendation selected = ordered.First(o => o.DisplayLabel == selectedOption);

        Crop? crop = await EnsureCropForFarmerAsync(selected.Produce);
        if (crop is null)
        {
            await DisplayAlert("Save Failed", _apiService.LastError ?? "Could not prepare crop data.", "OK");
            return;
        }

        string weightUnit = MeasurementService.WeightUnit;
        string? quantityText = await DisplayPromptAsync("Add Crop to Field", $"Quantity ({weightUnit})", initialValue: "1", keyboard: Keyboard.Numeric);
        if (!TryParseDecimalInput(quantityText, out decimal quantityValue) || quantityValue <= 0)
        {
            await DisplayAlert("Validation", "Quantity must be a positive number.", "OK");
            return;
        }

        decimal quantity = MeasurementService.ToTons(quantityValue);

        string? plantingDateText = await DisplayPromptAsync("Add Crop to Field", "Planting date (yyyy-MM-dd)", initialValue: DateTime.Today.ToString("yyyy-MM-dd"));
        if (!DateTime.TryParse(plantingDateText, out DateTime plantingDate))
        {
            await DisplayAlert("Validation", "Invalid planting date.", "OK");
            return;
        }

        DateTime suggestedHarvest = plantingDate.AddDays(selected.Produce.AvgDaysToHarvest);
        string? harvestDateText = await DisplayPromptAsync("Add Crop to Field", "Expected harvest date (yyyy-MM-dd)", initialValue: suggestedHarvest.ToString("yyyy-MM-dd"));
        if (!DateTime.TryParse(harvestDateText, out DateTime harvestDate) || harvestDate <= plantingDate)
        {
            await DisplayAlert("Validation", "Harvest date must be after planting date.", "OK");
            return;
        }

        bool ok = await _apiService.AddFieldCropAsync(field.FieldId, crop.CropId, quantity, plantingDate, harvestDate);
        if (!ok)
        {
            await DisplayAlert("Save Failed", _apiService.LastError ?? "Could not add crop to field.", "OK");
            return;
        }

        await DisplayAlert("Success", $"{crop.Name} added to {field.Name}.", "OK");
        await LoadFieldsAsync();
    }

    private async Task RemoveCropFromFieldAsync(List<FieldCrop> plantings)
    {
        if (plantings.Count == 0)
        {
            await DisplayAlert("Nothing to Remove", "This field has no crops assigned yet.", "OK");
            return;
        }

        List<string> options = plantings
            .Select(p => $"{p.CropName} - {MeasurementService.FormatWeight(p.QuantityInTons)} (Harvest: {p.HarvestDate:yyyy-MM-dd})")
            .ToList();

        string? selected = await DisplayActionSheet("Select crop to remove", "Cancel", null, options.ToArray());
        if (string.IsNullOrWhiteSpace(selected) || selected == "Cancel")
        {
            return;
        }

        int selectedIndex = options.FindIndex(o => o == selected);
        if (selectedIndex < 0)
        {
            return;
        }

        FieldCrop target = plantings[selectedIndex];
        bool confirm = await DisplayAlert("Remove Crop", $"Remove {target.CropName} from this field?", "Remove", "Cancel");
        if (!confirm)
        {
            return;
        }

        bool ok = await _apiService.DeleteFieldCropAsync(target.FieldCropId);
        if (!ok)
        {
            await DisplayAlert("Remove Failed", _apiService.LastError ?? "Could not remove crop from field.", "OK");
            return;
        }

        await DisplayAlert("Removed", $"{target.CropName} was removed from the field.", "OK");
        await LoadFieldsAsync();
    }

    private async Task<Crop?> EnsureCropForFarmerAsync(ProduceKnowledge produce)
    {
        Crop? existing = _allCrops.FirstOrDefault(c => c.Name.Equals(produce.Name, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            return existing;
        }

        decimal defaultYieldPerAcre = ResolveDefaultYieldPerAcre(produce);
        decimal optimalTemp = Convert.ToDecimal(produce.IdealTempC, CultureInfo.InvariantCulture);
        bool added = await _apiService.AddCropAsync(
            produce.Name,
            ResolveUnitForProduce(produce),
            produce.AvgDaysToHarvest,
            defaultYieldPerAcre,
            optimalTemp);

        if (!added)
        {
            return null;
        }

        await LoadCropsCacheAsync();
        return _allCrops.FirstOrDefault(c => c.Name.Equals(produce.Name, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<List<ProduceKnowledge>> EnsureProduceCatalogAsync()
    {
        if (_produceCatalog.Count > 0)
        {
            return _produceCatalog;
        }

        List<ProduceKnowledge> produce = await _apiService.GetProduceCatalogAsync();
        _produceCatalog.Clear();
        _produceCatalog.AddRange(produce);
        return _produceCatalog;
    }

    private async Task<List<ProduceRecommendation>> BuildProduceRecommendationsAsync(List<ProduceKnowledge> produceCatalog)
    {
        double currentTemp = await TryGetFarmCurrentTempAsync() ?? 20d;

        List<ProduceKnowledge> recommended;
        try
        {
            recommended = await _apiService.GetRecommendedProducesAsync(currentTemp);
        }
        catch
        {
            recommended = new List<ProduceKnowledge>();
        }

        HashSet<int> recommendedIds = recommended.Select(p => p.Id).ToHashSet();
        List<ProduceKnowledge> ordered = new();
        ordered.AddRange(recommended);
        ordered.AddRange(produceCatalog
            .Where(p => !recommendedIds.Contains(p.Id))
            .OrderBy(p => p.Name));

        return ordered.Select((produce, index) => new ProduceRecommendation
        {
            Produce = produce,
            DisplayLabel = index < 6
                ? $"[Recommended] {produce.Name} ({produce.Category})"
                : $"{produce.Name} ({produce.Category})"
        }).ToList();
    }

    private async Task<double?> TryGetFarmCurrentTempAsync()
    {
        try
        {
            if (_regions.Count == 0)
            {
                return null;
            }

            List<decimal> allRecentTemps = new();
            foreach (CropRegion region in _regions)
            {
                List<WeatherLog> logs = await _apiService.GetWeatherLogsAsync(region.RegionId);
                allRecentTemps.AddRange(logs
                    .Where(l => l.DateRecorded.Date >= DateTime.Today.AddDays(-3))
                    .Select(l => l.Temperature));
            }

            if (allRecentTemps.Count == 0)
            {
                return null;
            }

            return Convert.ToDouble(allRecentTemps.Average(), CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    private string ResolveRegionName(int regionId)
    {
        return _regions.FirstOrDefault(r => r.RegionId == regionId)?.Name ?? $"Region #{regionId}";
    }

    private FieldCardItem ToCard(Field field)
    {
        return new FieldCardItem
        {
            FieldId = field.FieldId,
            Name = field.Name,
            Location = field.Location,
            Acres = field.Acres,
            RegionId = field.RegionId,
            RegionName = ResolveRegionName(field.RegionId),
            PlantingsCount = _fieldPlantings.GetValueOrDefault(field.FieldId),
            CropsPreview = _fieldCropsPreview.GetValueOrDefault(field.FieldId, "No crops assigned yet.")
        };
    }

    private static bool TryParseDecimalInput(string? text, out decimal value)
    {
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
        {
            return true;
        }

        return decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    private static string ResolveUnitForProduce(ProduceKnowledge produce)
    {
        string category = produce.Category.ToLowerInvariant();
        return category.Contains("grain") || category.Contains("oilseed")
            ? "tons"
            : "kg";
    }

    private static decimal ResolveDefaultYieldPerAcre(ProduceKnowledge produce)
    {
        string category = produce.Category.ToLowerInvariant();
        if (category.Contains("grain"))
        {
            return 3.8m;
        }

        if (category.Contains("fruit"))
        {
            return 12.5m;
        }

        if (category.Contains("oilseed"))
        {
            return 1.7m;
        }

        return 7.2m;
    }

    private async void OnAddFieldClicked(object sender, EventArgs e)
    {
        await AddOrEditFieldAsync(null);
    }

    private async void OnEditFieldSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is FieldCardItem field)
        {
            await AddOrEditFieldAsync(field);
        }
    }

    private async void OnDeleteFieldSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipeItem || swipeItem.CommandParameter is not FieldCardItem field)
        {
            return;
        }

        int farmerId = await _authService.GetFarmerIdAsync();
        bool confirm = await DisplayAlert("Delete Field", $"Delete {field.Name}?", "Delete", "Cancel");
        if (!confirm)
        {
            return;
        }

        bool ok = await _apiService.DeleteFieldAsync(farmerId, field.FieldId);
        if (!ok)
        {
            await DisplayAlert("Delete Failed", _apiService.LastError ?? "Could not delete field.", "OK");
            return;
        }

        await LoadFieldsAsync();
    }

    private async void OnFieldSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not FieldCardItem field)
        {
            return;
        }

        string action = await DisplayActionSheet(field.Name, "Close", null, "View details", "Manage crops in this field", "Edit field");
        if (action == "View details")
        {
            await DisplayAlert(
                field.Name,
                $"Location: {field.Location}\nAcreage: {MeasurementService.FormatArea(field.Acres)}\nRegion: {field.RegionName}\nCurrent crops: {field.PlantingsCount}",
                "Close");
        }
        else if (action == "Manage crops in this field")
        {
            await ManageFieldCropsAsync(field);
        }
        else if (action == "Edit field")
        {
            await AddOrEditFieldAsync(field);
        }

        FieldsCollectionView.SelectedItem = null;
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFieldFilters();
    }

    private void OnFilterChanged(object sender, EventArgs e)
    {
        ApplyFieldFilters();
    }

    private async void OnRefreshRequested(object sender, EventArgs e)
    {
        await LoadAllAsync();
    }

    private sealed class FieldCardItem
    {
        public int FieldId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public decimal Acres { get; init; }
        public int RegionId { get; init; }
        public string RegionName { get; init; } = string.Empty;
        public int PlantingsCount { get; init; }
        public string CropsPreview { get; init; } = string.Empty;

        public string AcresText => MeasurementService.FormatArea(Acres);
        public string PlantingsText => $"{PlantingsCount} crops";
    }

    private sealed class ProduceRecommendation
    {
        public ProduceKnowledge Produce { get; init; } = new();
        public string DisplayLabel { get; init; } = string.Empty;
    }
}
