using CropTrackApp.Services;

namespace CropTrackApp.Pages;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;

    public LoginPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ErrorLabel.Text = "Please fill in all fields.";
            ErrorLabel.IsVisible = true;
            return;
        }

        LoginButton.IsEnabled = false;
        LoginButton.Text = "Logging in...";
        ErrorLabel.IsVisible = false;

        bool success = await _apiService.LoginAsync(email, password);

        if (success)
        {
            Application.Current.MainPage = new AppShell();
        }
        else
        {
            ErrorLabel.Text = "Invalid email or password.";
            ErrorLabel.IsVisible = true;
            LoginButton.IsEnabled = true;
            LoginButton.Text = "Login";
        }
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage(_apiService));
    }
}