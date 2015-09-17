using Momentum.App.Models;
using System;
using System.Text.RegularExpressions;
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
        IWebDownloadService _webDownloadService;

        /// <summary>
        /// Creates a BingImageService instance.
        /// </summary>
        /// <param name="regionLanguageIso">The region language in ISO format.</param>
        public BingImageService(string regionLanguageIso = "en-US")
        {
            _httpService = new HttpService();
            _serializationService = new DataContractSerializationService();
            _webDownloadService = new WebDownloadService();

            RegionLanguageIso = regionLanguageIso;
        }

        public async Task<BingImageResult> LoadImageAsync()
        {
            var jsonString = await _httpService.GetAsync(new Uri(IMAGE_SOURCE_URI + RegionLanguageIso, UriKind.Absolute));

            if (!string.IsNullOrEmpty(jsonString))
            {
                var imageModel = _serializationService.DeserializeJson<BingImageModel>(jsonString);
                
                if (imageModel.images.Count > 0)
                {
                    var imageItem = imageModel.images[0];

                    //var bitmapImage = new BitmapImage();
                    //bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache; // TODO: what is its purpose?
                    //bitmapImage.UriSource = new Uri(BASE_URI + imageUrl, UriKind.Absolute);
                    //return bitmapImage;

                    // download image
                    var imageFile = await _webDownloadService.DownloadAsync(new Uri(BASE_URI + imageItem.url, UriKind.Absolute));

                    if (imageFile != null)
                    {
                        // create image source
                        var bitmapImage = new BitmapImage();
                        bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache; // TODO: what is its purpose?
                        bitmapImage.UriSource = new Uri(imageFile.Path);

                        var result = new BingImageResult()
                        {
                            ImageSource = bitmapImage,
                        };

                        // trim copyright
                        Regex regName = new Regex(@"\((.*)\)");
                        Match match = regName.Match(imageItem.copyright);
                        if (match.Success)
                        {
                            result.Copryright = match.Groups[1].Value;
                        }

                        return result;
                    }
                }
            }

            return null;
        }

        public string RegionLanguageIso { get; set; }
    }
}
