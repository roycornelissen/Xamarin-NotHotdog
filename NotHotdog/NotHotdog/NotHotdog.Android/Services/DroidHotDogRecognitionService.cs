using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using NotHotdog.Model;
using NotHotdog.Services;
using Org.Tensorflow.Contrib.Android;

namespace NotHotdog.Droid.Services
{
	public class DroidHotDogRecognitionService : IHotDogRecognitionService
	{
		TensorFlowInferenceInterface inferenceInterface;
		List<string> labels;

		public DroidHotDogRecognitionService()
		{
			var assets = Application.Context.Assets;
			inferenceInterface = new TensorFlowInferenceInterface(assets, "model.pb");
			using (var sr = new StreamReader(assets.Open("labels.txt")))
			{
				labels = sr.ReadToEnd()
								   .Split('\n')
								   .Select(s => s.Trim())
								   .Where(s => !string.IsNullOrEmpty(s))
								   .ToList();
			}
		}

		public Task<RecognizedHotdog> CheckImageForDescription(byte[] imagesBytes)
		{
			try
			{
				using (var bitmap = BitmapFactory.DecodeByteArray(imagesBytes, 0, imagesBytes.Length))
				using (var resizedBitmap = Bitmap.CreateScaledBitmap(bitmap, 227, 227, false)
					  .Copy(Bitmap.Config.Argb8888, false))
				{
					var floatValues = new float[227 * 227 * 3];
					var intValues = new int[227 * 227];
					resizedBitmap.GetPixels(intValues, 0, 227, 0, 0, 227, 227);
					for (int i = 0; i < intValues.Length; ++i)
					{
						var val = intValues[i];
						floatValues[i * 3 + 0] = ((val & 0xFF) - 104);
						floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - 117);
						floatValues[i * 3 + 2] = (((val >> 16) & 0xFF) - 123);
					}

					var outputs = new float[labels.Count];
					inferenceInterface.Feed("Placeholder", floatValues, 1, 227, 227, 3);
					inferenceInterface.Run(new[] { "loss" });
					inferenceInterface.Fetch("loss", outputs);

					var hotdogCertainty = outputs[0]; // index of "hotdog" label
					if (hotdogCertainty > 0.75f)
					{
						return Task.FromResult(new RecognizedHotdog
						{
							Categories = new List<string> { "hotdog" },
							Certainty = hotdogCertainty,
							Description = null,
							Hotdog = true,
							Tags = new List<string> { "hotdog" }
						});
					}
					else
					{
						return Task.FromResult(new RecognizedHotdog
						{
							Categories = new List<string> { labels[1] },
							Certainty = hotdogCertainty,
							Description = "Not sure what this is",
							Hotdog = false,
							Tags = new List<string> { labels[1] }
						});
					}
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
				return null;
			}
		}
	}
}
