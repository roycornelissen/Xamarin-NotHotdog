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

namespace NotHotdog.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
        }

        RecognizedHotdog hotdog = new RecognizedHotdog() { Hotdog = false};
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
            set {SetProperty(ref picture, value); }
        }


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
					return;
				}

                IsBusy = true;
				var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
				{
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
                        if(apiresult.description.tags.Any(t => t == "hotdog") || (apiresult.description.tags.Any(t => t == "hot") && apiresult.description.tags.Any(t => t == "dog")))
                        {
                            recognizedHotdog.Hotdog = true;
                        }
                        if (apiresult.description.captions.Any())
                        {
                            recognizedHotdog.Description = apiresult.description.captions.FirstOrDefault().text;
                            recognizedHotdog.Certainty = apiresult.description.captions.FirstOrDefault().confidence;
                        }
                        recognizedHotdog.Tags = apiresult.tags.Select(t => t.name).ToList();

                        Hotdog = recognizedHotdog;
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
    }
}
