using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using NotHotdog.Model;
using NotHotdog.Services;
using UIKit;

namespace NotHotdog.iOS.Services
{
	public class SqueezeNetHotDogRecognitionService : IHotDogRecognitionService
	{
		static readonly SqueezeNet model = new SqueezeNet();

		public async Task<RecognizedHotdog> CheckImageForDescription(Stream imageStream)
		{
			return await Task.Run(() =>
			{
				try
				{
					var image = UIImage.LoadFromData(NSData.FromStream(imageStream));
					var buffer = image.Scale(new CGSize(227, 227)).ToCVPixelBuffer();

					var output = model.GetPrediction(buffer, out NSError error);
					if (error != null)
					{
						Console.WriteLine(error.DebugDescription);
						return null;
					}

					var isHotdog = output.ClassLabel.Contains("hotdog") || output.ClassLabel.Contains("hot dog");

					var dict = new Dictionary<string, double>();
					foreach (var item in output.ClassLabelProbs)
					{
						dict.Add(item.Key.ToString(), ((NSNumber)item.Value).DoubleValue);
					}
					var tags = dict.OrderByDescending(kv => kv.Value).Take(5).Select(kv => kv.Key).ToList();
					var result = new RecognizedHotdog
					{
						Hotdog = isHotdog,
						Certainty = dict.Max(kv => kv.Value),
						Tags = tags,
						Description = output.ClassLabel
					};

					return result;
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
			});
		}
	}
}
