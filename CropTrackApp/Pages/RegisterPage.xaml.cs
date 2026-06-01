using CropTrackApp.Services;

namespace CropTrackApp.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly ApiService _apiService;

    public RegisterPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string? name = NameEntry.Text?.Trim();
        string? email = EmailEntry.Text?.Trim();
        string? password = PasswordEntry.Text;
        string? confirmPassword = ConfirmPasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            ErrorLabel.Text = "Please fill in all fields.";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (password != confirmPassword)
        {
            ErrorLabel.Text = "Passwords do not match.";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (password.Length < 6)
        {
            ErrorLabel.Text = "Password must be at least 6 characters.";
            ErrorLabel.IsVisible = true;
            return;
        }

        RegisterButton.IsEnabled = false;
        RegisterButton.Text = "Creating account...";
        ErrorLabel.IsVisible = false;

        try
        {
            bool success = await _apiService.RegisterAsync(name, email, password);
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
                    400 => string.IsNullOrWhiteSpace(err) ? "Invalid registration data. Please check your information." : err,
                    409 => string.IsNullOrWhiteSpace(err) ? "This email is already registered. Please use a different email or login." : err,
                    500 => string.IsNullOrWhiteSpace(err) ? "Server error. Please try again later." : err,
                    0 => string.IsNullOrWhiteSpace(err)
                        ? "Cannot connect to API. Start the backend and check emulator access to port 5075."
                        : err,
                    _ => string.IsNullOrWhiteSpace(err) ? "Registration failed. Please try again." : err
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
            RegisterButton.IsEnabled = true;
            RegisterButton.Text = "Create Account";
        }
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
