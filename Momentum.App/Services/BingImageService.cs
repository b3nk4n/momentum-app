using Momentum.App.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UWPCore.Framework.Data;
using UWPCore.Framework.Networking;
using UWPCore.Framework.Storage;
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

        public const string BACKGROUND_IMAGE_LOCAL_NAME = "back_image.jpg";

        IHttpService _httpService;
        ISerializationService _serializationService;
        IWebDownloadService _webDownloadService;

        /// <summary>
        /// The time stamp the last time an image has been loaded successfully.
        /// </summary>
        private static StoredObjectBase<int> BackgroundImageDay = new LocalObject<int>("_backImageDay_", -1);

        /// <summary>
        /// The saved copyright of the last loaded background image for reuse.
        /// </summary>
        private static StoredObjectBase<string> LastBackgroundImageCopyright = new LocalObject<string>("_backImageCopyright_", "© Unknown");

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
            // reuse the image, when we are at the same day
            if (DateTime.Now.Day == BackgroundImageDay.Value)
            {
                return new BingImageResult()
                {
                    Copryright = LastBackgroundImageCopyright.Value,
                    ImageSource = new BitmapImage(new Uri(IOConstants.APPDATA_LOCAL_SCHEME + "/" + BACKGROUND_IMAGE_LOCAL_NAME))
                };
            }

            var jsonString = await _httpService.GetAsync(new Uri(IMAGE_SOURCE_URI + RegionLanguageIso, UriKind.Absolute));

            if (!string.IsNullOrEmpty(jsonString))
            {
                var imageModel = _serializationService.DeserializeJson<BingImageModel>(jsonString);
                
                if (imageModel.images.Count > 0)
                {
                    var imageItem = imageModel.images[0];

                    // download image
                    var imageFile = await _webDownloadService.DownloadAsync(new Uri(BASE_URI + imageItem.url, UriKind.Absolute), BACKGROUND_IMAGE_LOCAL_NAME);

                    if (imageFile != null)
                    {
                        BackgroundImageDay.Value = DateTime.Now.Day;

                        // create image source
                        var bitmapImage = new BitmapImage(new Uri(imageFile.Path));

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

                        LastBackgroundImageCopyright.Value = result.Copryright;

                        return result;
                    }
                }
            }

            return GetDefaultImage();
        }

        public string RegionLanguageIso { get; set; }

        /// <summary>
        /// Gets the default background image.
        /// </summary>
        /// <returns>Returns the default background image.</returns>
        private BingImageResult GetDefaultImage()
        {
            return new BingImageResult()
            {
                Copryright = "© Dave Lane/NASA",
                ImageSource = new BitmapImage(new Uri(IOConstants.APPX_SCHEME + "/Assets/Images/CapReefMilky_EN-US.jpg"))
            };
        }
    }
}
