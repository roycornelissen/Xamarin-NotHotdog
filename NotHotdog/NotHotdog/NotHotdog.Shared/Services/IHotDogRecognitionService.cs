using System.IO;
using System.Threading.Tasks;
using NotHotdog.Model;

namespace NotHotdog.Services
{
	public interface IHotDogRecognitionService
    {
		Task<RecognizedHotdog> CheckImageForDescription(Stream imageStream);
	}
}
