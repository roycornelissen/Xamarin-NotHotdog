using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NotHotdog.Model;
using NotHotdog.Model.CustomVisionJson;

namespace NotHotdog.Services
{
	public class CustomVisionHotDogRecognitionService : IHotDogRecognitionService
	{
		private static readonly HttpClient client = new HttpClient();
		private const string predictionUrl = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/95d44082-d30a-422b-a392-0ecf778809dc/image?iterationId=ba0ef288-a06b-4559-b7be-160e9f678d76";

		static CustomVisionHotDogRecognitionService()
		{
			client = new HttpClient
			{
				BaseAddress = new Uri(predictionUrl)
			};
			client.DefaultRequestHeaders.Add("Prediction-Key", Constants.ApiKeys.CUSTOMVISION_PREDICTIONKEY);
		}

		public async Task<RecognizedHotdog> CheckImageForDescription(byte[] imagesBytes)
		{
			using (ByteArrayContent content = new ByteArrayContent(imagesBytes))
			{
				content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

				var response = await client.PostAsync(predictionUrl, content);
				string contentString = await response.Content.ReadAsStringAsync();
				Debug.WriteLine(contentString);

				var apiResponse = JsonConvert.DeserializeObject<CustomVisionResponse>(contentString);
				if (apiResponse != null)
				{
					if (apiResponse.Predictions?.Any() ?? false)
					{
						var mostConfidence = apiResponse.Predictions.OrderByDescending(p => p.Probability).First();
						if (mostConfidence.Probability > 0.75f && mostConfidence.TagName == "hotdog")
						{
							return new RecognizedHotdog
							{
								Certainty = Convert.ToDouble(mostConfidence.Probability),
								Categories = new List<string> { "Food" },
								Description = "It's a hotdog",
								Hotdog = true,
								Tags = apiResponse.Predictions.Select(p => p.TagName).ToList()
							};
						}
					}
				}
				return new RecognizedHotdog
				{
					Certainty = 0d,
					Categories = new List<string> { "Unknown" },
					Description = "It's not a hotdog; I don't know what this is",
					Hotdog = false,
					Tags = new List<string> { "unknown" }
				};
			}
		}
	}
}
