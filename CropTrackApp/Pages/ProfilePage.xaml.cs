using CropTrackApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Storage;

namespace CropTrackApp.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly AuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    public ProfilePage()
    {
        InitializeComponent();

        _serviceProvider = IPlatformApplication.Current?.Services
            ?? throw new InvalidOperationException("Application services are not available.");
        _authService = _serviceProvider.GetRequiredService<AuthService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        NameLabel.Text = await _authService.GetFarmerNameAsync() ?? "Unknown Farmer";
        EmailLabel.Text = await _authService.GetFarmerEmailAsync() ?? "No email available";
        LoadSettings();
        BindWeeklyCheckStatus();
    }

    private void LoadSettings()
    {
        TemperatureUnitPicker.SelectedIndex = Preferences.Get("temperature_unit_index", 0);
        AreaUnitPicker.SelectedIndex = Preferences.Get("area_unit_index", 0);
        WeightUnitPicker.SelectedIndex = Preferences.Get("weight_unit_index", 0);
        RainfallUnitPicker.SelectedIndex = Preferences.Get("rainfall_unit_index", 0);

        HarvestReminderSwitch.IsToggled = Preferences.Get("notify_harvest", true);
        WeatherAlertSwitch.IsToggled = Preferences.Get("notify_weather", true);
        PriceUpdateSwitch.IsToggled = Preferences.Get("notify_price", true);
        DarkModeSwitch.IsToggled = Preferences.Get("display_dark_mode", false);
    }

    private async void OnSaveSettingsClicked(object sender, EventArgs e)
    {
        Preferences.Set("temperature_unit_index", TemperatureUnitPicker.SelectedIndex < 0 ? 0 : TemperatureUnitPicker.SelectedIndex);
        Preferences.Set("area_unit_index", AreaUnitPicker.SelectedIndex < 0 ? 0 : AreaUnitPicker.SelectedIndex);
        Preferences.Set("weight_unit_index", WeightUnitPicker.SelectedIndex < 0 ? 0 : WeightUnitPicker.SelectedIndex);
        Preferences.Set("rainfall_unit_index", RainfallUnitPicker.SelectedIndex < 0 ? 0 : RainfallUnitPicker.SelectedIndex);

        Preferences.Set("notify_harvest", HarvestReminderSwitch.IsToggled);
        Preferences.Set("notify_weather", WeatherAlertSwitch.IsToggled);
        Preferences.Set("notify_price", PriceUpdateSwitch.IsToggled);
        Preferences.Set("display_dark_mode", DarkModeSwitch.IsToggled);
        Preferences.Set("profile_last_review_date", DateTime.Today.ToString("yyyy-MM-dd"));
        BindWeeklyCheckStatus();

        await DisplayAlert("Saved", "Settings updated successfully.", "OK");
    }

    private void BindWeeklyCheckStatus()
    {
        string? savedDateText = Preferences.Get("profile_last_review_date", string.Empty);
        if (!DateTime.TryParse(savedDateText, out DateTime lastReview))
        {
            WeeklyCheckLabel.Text = "Weekly check: set your preferences once this week.";
            return;
        }

        int days = (DateTime.Today - lastReview.Date).Days;
        if (days >= 7)
        {
            WeeklyCheckLabel.Text = $"Weekly check due: last review was {days} day(s) ago.";
            return;
        }

        WeeklyCheckLabel.Text = $"Weekly check done. Next review in {7 - days} day(s).";
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "Cancel");
        if (!confirm)
        {
            return;
        }

        _authService.Logout();

        if (Application.Current is null)
        {
            return;
        }

        LoginPage loginPage = _serviceProvider.GetRequiredService<LoginPage>();
        Application.Current.MainPage = new NavigationPage(loginPage);
    }
}
