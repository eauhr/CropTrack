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
        string name = NameEntry.Text?.Trim();
        string email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text;
        string confirmPassword = ConfirmPasswordEntry.Text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
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

        bool success = await _apiService.RegisterAsync(name, email, password);

        if (success)
        {
            Application.Current.MainPage = new AppShell();
        }
        else
        {
            ErrorLabel.Text = "Registration failed. Email may already be in use.";
            ErrorLabel.IsVisible = true;
            RegisterButton.IsEnabled = true;
            RegisterButton.Text = "Create Account";
        }
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

