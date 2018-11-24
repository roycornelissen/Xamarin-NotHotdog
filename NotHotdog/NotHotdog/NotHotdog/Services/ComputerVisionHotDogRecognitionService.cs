using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NotHotdog.Model;

namespace NotHotdog.Services
{
	public class ComputerVisionHotDogRecognitionService : IHotDogRecognitionService
    {
        public ComputerVisionHotDogRecognitionService()
        {
        }

		public async Task<RecognizedHotdog> CheckImageForDescription(byte[] imagesBytes)
		{
			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Constants.ApiKeys.COMPUTERVISION_APIKEY);

			string uri = "https://westeurope.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Categories,Tags,Description&language=en";

			HttpResponseMessage response;

			using (ByteArrayContent content = new ByteArrayContent(imagesBytes))
			{
                //sent byte array to cognitive services API
				content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
				response = await client.PostAsync(uri, content);

                //read response as string and deserialize
				string contentString = await response.Content.ReadAsStringAsync();
				CognitiveResults apiresult = JsonConvert.DeserializeObject<CognitiveResults>(contentString);

                //check if result contains word hotdog or parts of it
				RecognizedHotdog recognizedHotdog = new RecognizedHotdog();
				if (apiresult.description.tags.Any(t => t == "hotdog") || (apiresult.description.tags.Any(t => t == "hot") && apiresult.description.tags.Any(t => t == "dog")))
				{
					recognizedHotdog.Hotdog = true;
				}
				if (apiresult.description.captions.Any())
				{
					recognizedHotdog.Description = apiresult.description.captions.FirstOrDefault().text;
					recognizedHotdog.Certainty = apiresult.description.captions.FirstOrDefault().confidence;
				}
				recognizedHotdog.Tags = apiresult.tags.Select(t => t.name).ToList();

				return recognizedHotdog;
			}
		}
    }
}
