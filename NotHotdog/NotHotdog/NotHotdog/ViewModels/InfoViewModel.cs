using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmHelpers;
using Newtonsoft.Json;
using NotHotdog.Model;
using Plugin.Media;
using Xamarin.Forms;
using System.Linq;
using Plugin.Share;
using Plugin.Share.Abstractions;
using Microsoft.Azure.Mobile.Analytics;
using System.Collections.Generic;
using Plugin.Media.Abstractions;
using NotHotdog.Services;

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

            Analytics.TrackEvent("open github event");
			await CrossShare.Current.OpenBrowser("https://github.com/Geertvdc/Xamarin-NotHotdog");
		}

        #endregion
    }
}
