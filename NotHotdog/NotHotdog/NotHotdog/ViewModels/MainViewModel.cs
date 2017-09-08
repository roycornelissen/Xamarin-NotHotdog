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

namespace NotHotdog.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
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

                    }
                    return;
                }

                IsBusy = true;
                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    CustomPhotoSize = 50,
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
                {
                    BinaryReader binaryReader = new BinaryReader(stream);
                    var imagesBytes = binaryReader.ReadBytes((int)stream.Length);
                    await CheckImageForDescription(imagesBytes);
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

        private async Task CheckImageForDescription(byte[] imagesBytes)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Constants.ApiKeys.COMPUTERVISION_APIKEY);

            string uri = "https://westeurope.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Categories,Tags,Description&language=en";

            HttpResponseMessage response;

            using (ByteArrayContent content = new ByteArrayContent(imagesBytes))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                response = await client.PostAsync(uri, content);
                string contentString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(contentString);

                CognitiveResults apiresult = JsonConvert.DeserializeObject<CognitiveResults>(contentString);

                RecognizedHotdog recognizedHotdog = new RecognizedHotdog();
                if (apiresult.description.tags.Any(t => t == "hotdog") || (apiresult.description.tags.Any(t => t == "hot") && apiresult.description.tags.Any(t => t == "dog")))
                {
                    recognizedHotdog.Hotdog = true;
                    Analytics.TrackEvent("Hotdog Scanned");
                }
                if (apiresult.description.captions.Any())
                {
                    Analytics.TrackEvent("Not Hotdog Scanned");
                    recognizedHotdog.Description = apiresult.description.captions.FirstOrDefault().text;
                    recognizedHotdog.Certainty = apiresult.description.captions.FirstOrDefault().confidence;
                }
                recognizedHotdog.Tags = apiresult.tags.Select(t => t.name).ToList();

                Hotdog = recognizedHotdog;
                Scanned = true;
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

		ICommand pickImageCommand;
		public ICommand PickImageCommand =>
			pickImageCommand ?? (pickImageCommand = new Command(async () => await ExecutePickImageAsync()));

		async Task ExecutePickImageAsync()
		{
            Error = false;
			if (IsBusy)
				return;

            //try
            //{
                if (CrossMedia.Current.IsPickPhotoSupported)
                {
                    IsBusy = true;
                    var photo = await CrossMedia.Current.PickPhotoAsync();

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

                        await CheckImageForDescription(imagesBytes);
                    }

                }

    //        }
    //        catch (Exception ex)
    //        {
    //            Error = true;
				//Analytics.TrackEvent("Exception while picking file from library", new Dictionary<string, string> {
					//{ "Exception", ex.Message },
					//{ "Exception Type", ex.GetType().ToString() },
					//{ "StackTrace", ex.StackTrace}});
            //}
            //finally
            //{
            //    IsBusy = false;
            //}
		}



#endregion
    }
}
