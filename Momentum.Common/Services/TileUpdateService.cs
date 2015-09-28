using Momentum.Common.Models;
using System;
using System.Threading.Tasks;
using UWPCore.Framework.Notifications;
using UWPCore.Framework.Notifications.Models;
using Windows.Globalization;

namespace Momentum.Common.Services
{
    /// <summary>
    /// Service class to update the live tile.
    /// </summary>
    public class TileUpdateService : ITileUpdateService
    {
        ITileService _tileService;
        IImageService _imageService;

        /// <summary>
        /// Creates a TileUpdateService instance.
        /// </summary>
        public TileUpdateService()
        {
            _tileService = new TileService();
            _imageService = new BingImageService(ApplicationLanguages.Languages[0]);
        }

        public async Task UpdateLiveTile(TodaysFocusModel latestFocus)
        {
            var imageResult = await _imageService.LoadImageAsync();

            var adaptiveTileModel = new AdaptiveTileModel()
            {
                Visual = new AdaptiveVisual()
                {
                    Branding = VisualBranding.NameAndLogo,
                    Bindings =
                    {
                        new AdaptiveBinding()
                        {
                            Template = VisualTemplate.TileMedium,
                            Children =
                            {
                                new AdaptiveImage()
                                {
                                    Source = imageResult?.ImagePath,
                                    Placement = ImagePlacement.Background
                                },
                                new AdaptiveText()
                                {
                                    Content = "Today's focus",
                                    HintStyle = TextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Content = latestFocus.Message,
                                    HintStyle = TextStyle.Caption,
                                    HintWrap = true
                                }
                            }
                        },
                        new AdaptiveBinding()
                        {
                            Template = VisualTemplate.TileWide,
                            Children =
                            {
                                new AdaptiveImage()
                                {
                                    Source = imageResult?.ImagePath,
                                    Placement = ImagePlacement.Background
                                },
                                new AdaptiveText()
                                {
                                    Content = "Tagesziel",
                                    HintStyle = TextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Content = latestFocus.Message,
                                    HintStyle = TextStyle.Body,
                                    HintWrap = true
                                }
                            }
                        }
                    }
                }
            };

            var tileNotification = _tileService.AdaptiveFactory.Create(adaptiveTileModel);

            tileNotification.ExpirationTime = DateTimeOffset.Now.Date.AddDays(1);
            _tileService.GetUpdaterForApplication().Update(tileNotification);
        }
    }
}
