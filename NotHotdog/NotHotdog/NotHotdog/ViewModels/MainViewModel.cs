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
using Microsoft.Azure.Mobile;

namespace NotHotdog.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        IHotDogRecognitionService _hotdogRecognitionService;
        INavigation _navigation;

        public MainViewModel(IHotDogRecognitionService hotdogRecognitionService, INavigation navigation)
        {
            _hotdogRecognitionService = hotdogRecognitionService;
            _navigation = navigation;
        }

        RecognizedHotdog hotdog = new RecognizedHotdog() { Hotdog = false };
        public RecognizedHotdog Hotdog
        {
            get { return hotdog; }
            set { SetProperty(ref hotdog, value); }
        }

        bool scanned;
        public bool Scanned
        {
            get { return scanned; }
            set { SetProperty(ref scanned, value); }
        }

        bool error;
        public bool Error
        {
            get { return error; }
            set { SetProperty(ref error, value); }
        }

        ImageSource picture;
        public ImageSource Picture
        {
            get { return picture; }
            set { SetProperty(ref picture, value); }
        }

        #region commands 

        ICommand checkImageCommand;
        public ICommand CheckImageCommand =>
            checkImageCommand ?? (checkImageCommand = new Command(async () => await ExecuteCheckImageAsync()));

        async Task ExecuteCheckImageAsync()
        {
            if (IsBusy)
                return;

            try
            {
                Error = false;
                Scanned = false;
                await CrossMedia.Current.Initialize();

				if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
				{
					if (!CrossMedia.Current.IsPickPhotoSupported)
					{

						return;
					}
				}

                IsBusy = true;
                var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
				{
					PhotoSize = PhotoSize.Small,
                    Directory = "Sample",
                    Name = "test.jpg"
                });

                if (file == null)
                    return;

                Picture = ImageSource.FromStream(() =>
                {
                    var stream = file.GetStream();
                    file.Dispose();
                    return stream;
                });

                using (var stream = file.GetStream())
				using (var binaryReader = new BinaryReader(stream))
				{
					var imagesBytes = binaryReader.ReadBytes((int)stream.Length);
					Hotdog = await _hotdogRecognitionService.CheckImageForDescription(imagesBytes);

                    if (Hotdog.Hotdog)
                    {
                        Analytics.TrackEvent("Hotdog Scanned", new Dictionary<string, string> {
                                { "Description", Hotdog.Description},
                                { "Certainty", Hotdog.Certainty.ToString() }});
                    }
                    else
                    {
                        string tags = "";
                        foreach (var Tag in Hotdog.Tags)
                        {
                            tags += Tag.ToString() + ", ";

                        }
                        Analytics.TrackEvent("Not Hotdog Scanned", new Dictionary<string, string> {
                                { "Description", Hotdog.Description},
                                { "Certainty", Hotdog.Certainty.ToString()},
                                { "Tags", tags}});
                    }

                    if (Hotdog.Tags.Count > 0)
                    {
                        CustomProperties properties = new CustomProperties();
                        properties.Set("tag", hotdog.Tags[0]);
                        MobileCenter.SetCustomProperties(properties);
                    }
					
                    Scanned = true;
                }

            }
            catch (Exception ex)
            {
                Error = true;  
				Analytics.TrackEvent("Exception while checking image from Camera", new Dictionary<string, string> {
                    { "Exception", ex.Message },
                    { "Exception Type", ex.GetType().ToString() },
                    { "StackTrace", ex.StackTrace}});
            }
            finally
            {
                IsBusy = false;
            }
        }

        ICommand shareCommand;
		public ICommand ShareCommand =>
			shareCommand ?? (shareCommand = new Command(async () => await ExecuteShareAsync()));

		async Task ExecuteShareAsync()
		{
			if (!CrossShare.IsSupported)
				return;

            Analytics.TrackEvent("Share button");

			await CrossShare.Current.Share(new ShareMessage
			{
				Title = "CFood App!",
				Text = "I'm using the CFood app to see what food i'm eating! You have to try it!",
				Url = "https://mobilefirstcloudfirst.net/cfood-app/"
			});
		}

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

        ICommand infoCommand;
        public ICommand InfoCommand =>
			infoCommand ?? (infoCommand = new Command(async () => await ExecuteInfoAsync()));

        async Task ExecuteInfoAsync()
		{
            await _navigation.PushAsync(new Views.InfoPage());
		}




		ICommand pickImageCommand;
		public ICommand PickImageCommand =>
			pickImageCommand ?? (pickImageCommand = new Command(async () => await ExecutePickImageAsync()));

		async Task ExecutePickImageAsync()
		{
            Error = false;
			if (IsBusy)
				return;

            try
            {
                if (CrossMedia.Current.IsPickPhotoSupported)
                {
                    IsBusy = true;
					var photo = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions(){ PhotoSize = PhotoSize.Small });

                    if (photo == null)
                    {
                        return;
                    }

                    Picture = ImageSource.FromStream(() =>
                    {
                        var stream = photo.GetStream();
                        photo.Dispose();
                        return stream;
                    });

                    using (var stream = photo.GetStream())
                    {
                        BinaryReader binaryReader = new BinaryReader(stream);
                        var imagesBytes = binaryReader.ReadBytes((int)stream.Length);

                        Hotdog = await _hotdogRecognitionService.CheckImageForDescription(imagesBytes);
                        if(Hotdog.Hotdog)
                        {
                            Analytics.TrackEvent("Hotdog Scanned", new Dictionary<string, string> {
                                { "Description", Hotdog.Description},
                                { "Certainty", Hotdog.Certainty.ToString() }});
                        }
                        else
                        {
                            string tags = "";
                            foreach(var Tag in Hotdog.Tags)
                            {
                                tags += Tag.ToString() + ", ";
                                
                            }
                            Analytics.TrackEvent("Not Hotdog Scanned", new Dictionary<string, string> {
                                { "Description", Hotdog.Description},
                                { "Certainty", Hotdog.Certainty.ToString()},
                                { "Tags", tags}});
                        }

						if (Hotdog.Tags.Count > 0)
						{
							CustomProperties properties = new CustomProperties();
							properties.Set("tag", hotdog.Tags[0]);
							MobileCenter.SetCustomProperties(properties);
						}

                        Scanned = true;
                    }

                }
            }
            catch (Exception ex)
            {
                Error = true;
				Analytics.TrackEvent("Exception while picking file from library", new Dictionary<string, string> {
					{ "Exception", ex.Message },
					{ "Exception Type", ex.GetType().ToString() },
					{ "StackTrace", ex.StackTrace}});
            }
            finally
            {
                IsBusy = false;
            }
		}



#endregion
    }
}
