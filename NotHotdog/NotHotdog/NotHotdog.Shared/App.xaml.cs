using NotHotdog.Services;
using Xamarin.Forms;

namespace NotHotdog
{
    public partial class App : Application
    {
        public App()
        {
			InitializeComponent();

			//DependencyService.Register<CustomVisionHotDogRecognitionService>();
			DependencyService.Register<ComputerVisionHotDogRecognitionService>();

			MainPage = new NavigationPage(new NotHotdog.MainPage());
        }

        protected override void OnStart()
        {            
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
