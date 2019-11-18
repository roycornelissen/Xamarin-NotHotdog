using MvvmHelpers;
using NotHotdog.Services;
using Plugin.Share;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NotHotdog.ViewModels
{
    public class InfoViewModel : BaseViewModel
    {
        IHotDogRecognitionService _hotdogRecognitionService;
        INavigation _navigation;

        public InfoViewModel(IHotDogRecognitionService hotdogRecognitionService, INavigation navigation)
        {
            _hotdogRecognitionService = hotdogRecognitionService;
            _navigation = navigation;
        }


        #region commands 

		ICommand githubCommand;
		public ICommand GithubCommand =>
			githubCommand ?? (githubCommand = new Command(async () => await ExecuteGithubAsync()));

		async Task ExecuteGithubAsync()
		{
			if (!CrossShare.IsSupported)
				return;

			await CrossShare.Current.OpenBrowser("https://github.com/Geertvdc/Xamarin-NotHotdog");
		}

        #endregion
    }
}
