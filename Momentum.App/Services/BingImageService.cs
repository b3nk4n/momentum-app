using Momentum.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPCore.Framework.Data;
using UWPCore.Framework.Networking;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Momentum.App.Services
{
    /// <summary>
    /// The Bing image service implementation to load the daily picture of the region.
    /// </summary>
    public class BingImageService : IImageService
    {
        public const string BASE_URI = "http://www.bing.com";
        public const string IMAGE_SOURCE_URI = BASE_URI + "/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=";

        IHttpService _httpService;
        ISerializationService _serializationService;

        /// <summary>
        /// Creates a BingImageService instance.
        /// </summary>
        /// <param name="regionLanguageIso">The region language in ISO format.</param>
        public BingImageService(string regionLanguageIso = "en-US")
        {
            _httpService = new HttpService();
            _serializationService = new DataContractSerializationService();

            RegionLanguageIso = regionLanguageIso;
        }

        public async Task<ImageSource> LoadImageAsync()
        {
            var jsonString = await _httpService.GetAsync(new Uri(IMAGE_SOURCE_URI + RegionLanguageIso, UriKind.Absolute));

            if (!string.IsNullOrEmpty(jsonString))
            {
                var imageModel = _serializationService.DeserializeJson<BingImageModel>(jsonString);
                
                if (imageModel.images.Count > 0)
                {
                    var imageUrl = imageModel.images[0].url;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache; // TODO: what is its purpose?
                    bitmapImage.UriSource = new Uri(BASE_URI + imageUrl, UriKind.Absolute);
                    return bitmapImage;
                }
            }

            return null;
        }

        public string RegionLanguageIso { get; set; }
    }
}
