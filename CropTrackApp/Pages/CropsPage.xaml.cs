using CropTrackApp.Models;
using CropTrackApp.Services;

namespace CropTrackApp.Pages;

public partial class CropsPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly List<Crop> _allCrops = new();
    private bool _isLoading;

    public CropsPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
        GrowthFilterPicker.SelectedIndex = 0;
        SortPicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCropsAsync();
    }

    private async Task LoadCropsAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        try
        {
            List<Crop> crops = await _apiService.GetCropsAsync();
            _allCrops.Clear();
            _allCrops.AddRange(crops);
            ApplyFilters();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Crops Error", ex.Message, "OK");
        }
        finally
        {
            _isLoading = false;
            CropsRefreshView.IsRefreshing = false;
        }
    }

    private void ApplyFilters()
    {
        IEnumerable<Crop> query = _allCrops;

        string search = CropSearchBar.Text?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        string growthFilter = GrowthFilterPicker.SelectedItem?.ToString() ?? "All growth";
        query = growthFilter switch
        {
            "Short (0-90)" => query.Where(c => c.AvgGrowthDays <= 90),
            "Medium (91-150)" => query.Where(c => c.AvgGrowthDays >= 91 && c.AvgGrowthDays <= 150),
            "Long (151+)" => query.Where(c => c.AvgGrowthDays >= 151),
            _ => query
        };

        string sort = SortPicker.SelectedItem?.ToString() ?? "Alphabetical";
        query = sort switch
        {
            "Highest yield" => query.OrderByDescending(c => c.YieldPerAcre),
            "Shortest growth" => query.OrderBy(c => c.AvgGrowthDays),
            _ => query.OrderBy(c => c.Name)
        };

        List<CropCardItem> cards = query.Select(c => new CropCardItem
        {
            CropId = c.CropId,
            Name = c.Name,
            Unit = c.Unit,
            AvgGrowthDays = c.AvgGrowthDays,
            YieldPerAcre = c.YieldPerAcre,
            OptimalTemperature = c.OptimalTemperature
        }).ToList();

        CropsCollectionView.ItemsSource = cards;
    }

    private async Task AddOrEditCropAsync(CropCardItem? existing)
    {
        bool isEdit = existing is not null;
        string title = isEdit ? "Edit Crop" : "Add Crop";

        string? name = await DisplayPromptAsync(title, "Crop name", initialValue: existing?.Name, maxLength: 100);
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        string? unit = await DisplayPromptAsync(title, "Unit (tons, kg, bushels)", initialValue: existing?.Unit ?? "tons", maxLength: 30);
        if (string.IsNullOrWhiteSpace(unit))
        {
            return;
        }

        string? growthText = await DisplayPromptAsync(title, "Average growth days", initialValue: existing?.AvgGrowthDays.ToString() ?? "120", keyboard: Keyboard.Numeric);
        if (!int.TryParse(growthText, out int growthDays) || growthDays <= 0)
        {
            await DisplayAlert("Validation", "Growth days must be a positive number.", "OK");
            return;
        }

        string? yieldText = await DisplayPromptAsync(title, "Yield per acre", initialValue: existing?.YieldPerAcre.ToString("0.##") ?? "0", keyboard: Keyboard.Numeric);
        if (!decimal.TryParse(yieldText, out decimal yieldPerAcre) || yieldPerAcre < 0)
        {
            await DisplayAlert("Validation", "Yield must be a valid number.", "OK");
            return;
        }

        string? tempText = await DisplayPromptAsync(title, "Optimal temperature (C)", initialValue: existing?.OptimalTemperature.ToString("0.#") ?? "20", keyboard: Keyboard.Numeric);
        if (!decimal.TryParse(tempText, out decimal optimalTemp))
        {
            await DisplayAlert("Validation", "Temperature must be a valid number.", "OK");
            return;
        }

        bool duplicateName = _allCrops.Any(c =>
            !string.Equals(c.Name, existing?.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(c.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (duplicateName)
        {
            await DisplayAlert("Duplicate", "A crop with this name already exists.", "OK");
            return;
        }

        bool ok = isEdit
            ? await _apiService.UpdateCropAsync(existing!.CropId, name.Trim(), unit.Trim(), growthDays, yieldPerAcre, optimalTemp)
            : await _apiService.AddCropAsync(name.Trim(), unit.Trim(), growthDays, yieldPerAcre, optimalTemp);

        if (!ok)
        {
            await DisplayAlert("Save Failed", _apiService.LastError ?? "Could not save crop.", "OK");
            return;
        }

        await DisplayAlert("Success", isEdit ? "Crop updated." : "Crop added.", "OK");
        await LoadCropsAsync();
    }

    private async void OnAddCropClicked(object sender, EventArgs e)
    {
        await AddOrEditCropAsync(null);
    }

    private async void OnEditCropSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is CropCardItem crop)
        {
            await AddOrEditCropAsync(crop);
        }
    }

    private async void OnDeleteCropSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipeItem || swipeItem.CommandParameter is not CropCardItem crop)
        {
            return;
        }

        bool confirm = await DisplayAlert("Delete Crop", $"Delete {crop.Name}?", "Delete", "Cancel");
        if (!confirm)
        {
            return;
        }

        bool ok = await _apiService.DeleteCropAsync(crop.CropId);
        if (!ok)
        {
            await DisplayAlert("Delete Failed", _apiService.LastError ?? "Could not delete crop.", "OK");
            return;
        }

        await LoadCropsAsync();
    }

    private async void OnCropSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not CropCardItem crop)
        {
            return;
        }

        await DisplayAlert(
            crop.Name,
            $"Unit: {crop.Unit}\nGrowth: {crop.AvgGrowthDays} days\nYield: {crop.YieldPerAcre:0.##} per acre\nOptimal temp: {crop.OptimalTemperature:0.#}C",
            "Close");

        CropsCollectionView.SelectedItem = null;
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void OnFilterChanged(object sender, EventArgs e)
    {
        ApplyFilters();
    }

    private async void OnRefreshRequested(object sender, EventArgs e)
    {
        await LoadCropsAsync();
    }

    private sealed class CropCardItem
    {
        public int CropId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Unit { get; init; } = string.Empty;
        public int AvgGrowthDays { get; init; }
        public decimal YieldPerAcre { get; init; }
        public decimal OptimalTemperature { get; init; }

        public string CropBadge => string.IsNullOrWhiteSpace(Name) ? "C" : Name[..1].ToUpperInvariant();
        public string Summary => $"{Unit} | {AvgGrowthDays} growth days";
        public string Metrics => $"Yield {YieldPerAcre:0.##}/acre | Opt temp {OptimalTemperature:0.#}C";
    }
}

