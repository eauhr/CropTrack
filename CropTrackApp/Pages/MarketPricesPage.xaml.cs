using CropTrackApp.Models;
using CropTrackApp.Services;
using Microsoft.Maui.Graphics;

namespace CropTrackApp.Pages;

public partial class MarketPricesPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly List<Crop> _crops = new();
    private readonly List<MarketPrice> _allPrices = new();
    private bool _isLoading;

    public MarketPricesPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
        TimeRangePicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await EnsureCropsLoadedAsync();
    }

    private async Task EnsureCropsLoadedAsync()
    {
        if (_crops.Count > 0)
        {
            return;
        }

        try
        {
            _crops.Clear();
            _crops.AddRange(await _apiService.GetCropsAsync());
            CropPicker.ItemsSource = _crops;
            if (_crops.Count > 0)
            {
                CropPicker.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Prices Error", ex.Message, "OK");
        }
    }

    private async Task LoadPricesAsync()
    {
        if (_isLoading)
        {
            return;
        }

        Crop? crop = CropPicker.SelectedItem as Crop;
        if (crop is null)
        {
            await DisplayAlert("Select Crop", "Please select a crop first.", "OK");
            return;
        }

        _isLoading = true;
        try
        {
            List<MarketPrice> prices = await _apiService.GetMarketPricesAsync(crop.CropId);
            _allPrices.Clear();
            _allPrices.AddRange(prices);
            ApplyFilters(crop);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Load Failed", ex.Message, "OK");
        }
        finally
        {
            _isLoading = false;
            PricesRefreshView.IsRefreshing = false;
        }
    }

    private void ApplyFilters(Crop crop)
    {
        IEnumerable<MarketPrice> filtered = _allPrices.OrderByDescending(p => p.DateRecorded);
        string range = TimeRangePicker.SelectedItem?.ToString() ?? "All time";
        DateTime now = DateTime.Today;
        filtered = range switch
        {
            "Last 30 days" => filtered.Where(p => p.DateRecorded.Date >= now.AddDays(-30)),
            "Last 90 days" => filtered.Where(p => p.DateRecorded.Date >= now.AddDays(-90)),
            _ => filtered
        };

        List<MarketPrice> list = filtered.ToList();
        List<PriceCardItem> cards = new();
        decimal? previous = null;

        foreach (MarketPrice item in list)
        {
            decimal? change = previous.HasValue ? item.PricePerTon - previous.Value : null;
            cards.Add(new PriceCardItem
            {
                MarketPriceId = item.MarketPriceId,
                CropId = item.CropId,
                CropName = crop.Name,
                PricePerTon = item.PricePerTon,
                DateRecorded = item.DateRecorded,
                Change = change
            });
            previous = item.PricePerTon;
        }

        PricesCollectionView.ItemsSource = cards;

        if (list.Count >= 2)
        {
            decimal latest = MeasurementService.PricePerPreferredWeight(list[0].PricePerTon);
            decimal previousPrice = MeasurementService.PricePerPreferredWeight(list[1].PricePerTon);
            decimal diff = latest - previousPrice;
            string direction = diff > 0 ? "up" : diff < 0 ? "down" : "flat";
            TrendLabel.Text = $"Trend: {direction} ({diff:0.##} per {MeasurementService.WeightUnit})";
        }
        else
        {
            TrendLabel.Text = "Trend: N/A";
        }
    }

    private async Task AddOrEditPriceAsync(PriceCardItem? existing)
    {
        Crop? selectedCrop = CropPicker.SelectedItem as Crop;
        if (selectedCrop is null)
        {
            await DisplayAlert("Select Crop", "Select a crop before adding a price.", "OK");
            return;
        }

        bool isEdit = existing is not null;
        string title = isEdit ? "Edit Market Price" : "Add Market Price";

        string weightUnit = MeasurementService.WeightUnit;
        string priceDefault = existing is null ? "0" : MeasurementService.PricePerPreferredWeight(existing.PricePerTon).ToString("0.##");
        string? priceText = await DisplayPromptAsync(title, $"Price per {weightUnit}", initialValue: priceDefault, keyboard: Keyboard.Numeric);
        if (!decimal.TryParse(priceText, out decimal priceValue) || priceValue <= 0)
        {
            await DisplayAlert("Validation", "Price must be a positive number.", "OK");
            return;
        }

        decimal pricePerTon = MeasurementService.PriceToPerTon(priceValue);

        string dateDefault = existing?.DateRecorded.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
        string? dateText = await DisplayPromptAsync(title, "Date (yyyy-MM-dd)", initialValue: dateDefault);
        if (!DateTime.TryParse(dateText, out DateTime dateRecorded))
        {
            await DisplayAlert("Validation", "Date format should be yyyy-MM-dd.", "OK");
            return;
        }

        bool ok = isEdit
            ? await _apiService.UpdateMarketPriceAsync(existing!.MarketPriceId, selectedCrop.CropId, pricePerTon, dateRecorded)
            : await _apiService.AddMarketPriceAsync(selectedCrop.CropId, pricePerTon, dateRecorded);

        if (!ok)
        {
            await DisplayAlert("Save Failed", _apiService.LastError ?? "Could not save market price.", "OK");
            return;
        }

        await LoadPricesAsync();
    }

    private async void OnFetchClicked(object sender, EventArgs e)
    {
        Crop? crop = CropPicker.SelectedItem as Crop;
        if (crop == null)
        {
            await DisplayAlert("Select Crop", "Please select a crop first.", "OK");
            return;
        }

        bool imported = await _apiService.FetchAndStoreUsdaPriceAsync(crop.CropId);
        if (!imported)
        {
            await DisplayAlert("Fetch Failed", _apiService.LastError ?? "Could not fetch USDA price.", "OK");
        }

        await LoadPricesAsync();
    }

    private async void OnAddPriceClicked(object sender, EventArgs e)
    {
        await AddOrEditPriceAsync(null);
    }

    private void OnFilterChanged(object sender, EventArgs e)
    {
        if (CropPicker.SelectedItem is Crop crop)
        {
            ApplyFilters(crop);
        }
    }

    private async void OnRefreshRequested(object sender, EventArgs e)
    {
        await LoadPricesAsync();
    }

    private async void OnEditPriceSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is PriceCardItem item)
        {
            await AddOrEditPriceAsync(item);
        }
    }

    private async void OnDeletePriceSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipeItem || swipeItem.CommandParameter is not PriceCardItem item)
        {
            return;
        }

        bool confirm = await DisplayAlert("Delete Price", $"Delete {item.PriceText} for {item.CropName}?", "Delete", "Cancel");
        if (!confirm)
        {
            return;
        }

        bool ok = await _apiService.DeleteMarketPriceAsync(item.MarketPriceId);
        if (!ok)
        {
            await DisplayAlert("Delete Failed", _apiService.LastError ?? "Could not delete market price.", "OK");
            return;
        }

        await LoadPricesAsync();
    }

    private sealed class PriceCardItem
    {
        public int MarketPriceId { get; init; }
        public int CropId { get; init; }
        public string CropName { get; init; } = string.Empty;
        public decimal PricePerTon { get; init; }
        public DateTime DateRecorded { get; init; }
        public decimal? Change { get; init; }

        public string PriceText => $"{MeasurementService.PricePerPreferredWeight(PricePerTon):0.##} / {MeasurementService.WeightUnit}";
        public string DateText => DateRecorded.ToString("dd MMM yyyy");
        public string ChangeText => Change.HasValue
            ? Change.Value > 0 ? $"Up {MeasurementService.PricePerPreferredWeight(Change.Value):0.##}" : Change.Value < 0 ? $"Down {MeasurementService.PricePerPreferredWeight(Math.Abs(Change.Value)):0.##}" : "No change"
            : "Baseline";
        public Color ChangeColor => Change.HasValue
            ? Change.Value > 0 ? Colors.Green : Change.Value < 0 ? Colors.OrangeRed : Colors.Gray
            : Colors.Gray;
    }
}
