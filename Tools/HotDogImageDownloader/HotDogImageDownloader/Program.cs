using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace HotDogImageDownloader
{
	class Program
	{
		static void Main(string[] args)
		{
			var urls = File.ReadAllLines("tacos.txt");
			//			var urls = File.ReadAllLines("hotdogs-good.txt");

			var client = new WebClient();

			foreach (var url in urls)
			{
				Console.Write($"{url} ... ");
				try
				{
					client.DownloadFile(new Uri(url), new FileInfo(url).Name);
					Console.WriteLine("OK");
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}
		}
	}
}
