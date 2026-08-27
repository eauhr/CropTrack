using CropTrackApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CropTrackApp.Pages;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IServiceProvider _serviceProvider;

    public LoginPage(ApiService apiService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _apiService = apiService;
        _serviceProvider = serviceProvider;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string? email = EmailEntry.Text?.Trim();
        string? password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ErrorLabel.Text = "Please fill in all fields.";
            ErrorLabel.IsVisible = true;
            return;
        }

        LoginButton.IsEnabled = false;
        LoginButton.Text = "Logging in...";
        ErrorLabel.IsVisible = false;

        try
        {
            bool success = await _apiService.LoginAsync(email, password);
            if (success)
            {
                if (Application.Current is null)
                {
                    ErrorLabel.Text = "App initialization failed. Please restart the app.";
                    ErrorLabel.IsVisible = true;
                    return;
                }

                ErrorLabel.IsVisible = false;
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                int status = _apiService.LastStatusCode;
                string? err = _apiService.LastError;

                string userMessage = status switch
                {
                    400 => string.IsNullOrWhiteSpace(err) ? "Invalid email or password format." : err,
                    401 => "Invalid email or password. Please try again.",
                    404 => "Account not found. Please check your email or register.",
                    500 => string.IsNullOrWhiteSpace(err) ? "Server error. Please try again later." : err,
                    0 => string.IsNullOrWhiteSpace(err)
                        ? "Cannot connect to API. Start the backend and check emulator access to port 5075."
                        : err,
                    _ => string.IsNullOrWhiteSpace(err) ? "Login failed. Please try again." : err
                };

                ErrorLabel.Text = userMessage;
                ErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Connection error: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoginButton.IsEnabled = true;
            LoginButton.Text = "Login";
        }
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        RegisterPage registerPage = _serviceProvider.GetRequiredService<RegisterPage>();
        await Navigation.PushAsync(registerPage);
    }
}
