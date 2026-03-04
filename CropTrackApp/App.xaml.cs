using CropTrackApp.Services;
using CropTrackApp.Pages;

namespace CropTrackApp
{
    public partial class App : Application
    {
        public App(ApiService apiService)
        {
            InitializeComponent();

            MainPage = new NavigationPage(new LoginPage(apiService));
        }
    }
}
