﻿using System.Threading.Tasks;

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
        /// Gets or sets the region language for the images to load.
        /// </summary>
        string RegionLanguageIso { get; set; }
    }
}
