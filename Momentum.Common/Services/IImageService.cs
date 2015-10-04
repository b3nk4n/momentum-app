using System.Threading.Tasks;
using Windows.Storage;

namespace Momentum.Common.Services
{
    /// <summary>
    /// Simple service interface to load wallpaper images from the web.
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Loads an image from the web.
        /// </summary>
        /// <returns>Returns the laoded image or NULL in case of an error.</returns>
        Task<BingImageResult> LoadImageAsync();

        /// <summary>
        /// Gets the background image as a file object.
        /// </summary>
        /// <returns>The background image file.</returns>
        Task<IStorageFile> GetBackgroundAsFileAsync();

        /// <summary>
        /// Gets or sets the region language for the images to load.
        /// </summary>
        string RegionLanguageIso { get; set; }
    }
}
