using MvvmHelpers;
using NotHotdog.Model;
using NotHotdog.Services;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Share;
using Plugin.Share.Abstractions;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NotHotdog.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
		readonly IHotDogRecognitionService _hotdogRecognitionService;
		readonly INavigation _navigation;

        public MainViewModel(IHotDogRecognitionService hotdogRecognitionService, INavigation navigation)
        {
            _hotdogRecognitionService = hotdogRecognitionService;
            _navigation = navigation;
        }

        RecognizedHotdog hotdog = new RecognizedHotdog { Hotdog = false };
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
				using (var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
					{
						PhotoSize = PhotoSize.Small,
						Directory = "Sample",
						Name = "test.jpg"
					}))
				{

					if (file == null)
						return;

					Picture = ImageSource.FromStream(() =>
					{
						var stream = file.GetStream();
						return stream;
					});

					using (var stream = file.GetStream())
					{
						Hotdog = await _hotdogRecognitionService.CheckImageForDescription(stream);
						Scanned = true;
					}
				}
            }
            catch (Exception ex)
            {
                Error = true;  
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
					using (var photo = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions() { PhotoSize = PhotoSize.Small }))
					{
						if (photo == null)
						{
							return;
						}

						Picture = ImageSource.FromStream(() =>
						{
							var stream = photo.GetStream();
							return stream;
						});

						using (var stream = photo.GetStream())
						{
							Hotdog = await _hotdogRecognitionService.CheckImageForDescription(stream);
							Scanned = true;
						}
					}
                }
            }
            catch (Exception ex)
            {
                Error = true;
            }
            finally
            {
                IsBusy = false;
            }
		}

#endregion
    }
}
