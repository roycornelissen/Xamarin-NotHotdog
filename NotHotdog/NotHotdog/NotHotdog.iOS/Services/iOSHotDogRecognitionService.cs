using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using NotHotdog.Model;
using NotHotdog.Services;
using UIKit;

namespace NotHotdog.iOS.Services
{
	public class iOSHotDogRecognitionService : IHotDogRecognitionService
	{
		static readonly NotHotDog model = new NotHotDog();

		public async Task<RecognizedHotdog> CheckImageForDescription(byte[] imagesBytes)
		{
			return await Task.Run(() =>
			{
				try
				{
					var image = UIImage.LoadFromData(NSData.FromArray(imagesBytes));
					var buffer = image.Scale(new CGSize(227, 227)).ToCVPixelBuffer();

					var output = model.GetPrediction(buffer, out NSError error);
					if (error != null)
					{
						Console.WriteLine(error.DebugDescription);
						return null;
					}

					var isHotdog = output.ClassLabel == "hotdog";
					var certainty = (NSNumber)output.Loss[output.ClassLabel];
					var result = new RecognizedHotdog
					{
						Hotdog = isHotdog,
						Certainty = certainty.DoubleValue,
						Tags = new List<string> { output.ClassLabel },
						Description = isHotdog ? "Yep, it's a hotdog" : "Not sure what this is"
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
