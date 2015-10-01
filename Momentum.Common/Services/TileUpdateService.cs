﻿using Momentum.Common.Models;
using System;
using System.Threading.Tasks;
using UWPCore.Framework.Common;
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
        private ITileService _tileService;
        private IImageService _imageService;
        private IQuoteService _quoteService;

        private Localizer _localizer = new Localizer("Momentum.Common");

        /// <summary>
        /// Creates a TileUpdateService instance.
        /// </summary>
        public TileUpdateService()
        {
            _tileService = new TileService();
            var language = ApplicationLanguages.Languages[0];
            _imageService = new BingImageService(language);
            _quoteService = new QuoteService(language);
        }

        public async Task UpdateLiveTile(TodaysFocusModel latestFocus)
        {
            var imageResult = await _imageService.LoadImageAsync();
            var quoteResult = await _quoteService.LoadQuoteAsync();

            var adaptiveLargeTemplate = new AdaptiveBinding()
            {
                Template = VisualTemplate.TileLarge,
                Children =
                            {
                                new AdaptiveImage()
                                {
                                    Source = imageResult?.ImagePath,
                                    Placement = ImagePlacement.Background
                                },

                                // todays focus message
                                new AdaptiveText()
                                {
                                    Content = _localizer.Get("TodaysFocus.Text"),
                                    HintStyle = TextStyle.CaptionSubtle,
                                    HintWrap = true,

                                },
                                new AdaptiveText()
                                {
                                    Content = latestFocus.Message,
                                    HintStyle = TextStyle.Body,
                                    HintWrap = true
                                },

                                CreateTextPlaceholder(),

                                // quote
                                new AdaptiveText()
                                {
                                    Content = quoteResult.quote,
                                    HintStyle = TextStyle.Caption,
                                    HintAlign = TextHintAlign.Center,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Content = quoteResult.author,
                                    HintStyle = TextStyle.CaptionSubtle,
                                    HintAlign = TextHintAlign.Center,
                                    HintWrap = false
                                }
                            }
            };

            // add additional text place holder for short messages
            if (latestFocus.Message != null && latestFocus.Message.Length < 80)
            {
                adaptiveLargeTemplate.Children.Insert(3, CreateTextPlaceholder());
            }
            if (latestFocus.Message != null && latestFocus.Message.Length < 50)
            {
                adaptiveLargeTemplate.Children.Insert(3, CreateTextPlaceholder());
            }
            if (latestFocus.Message != null && latestFocus.Message.Length < 25)
            {
                adaptiveLargeTemplate.Children.Insert(3, CreateTextPlaceholder());
            }

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
                                    Content = _localizer.Get("TodaysFocus.Text"),
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
                                    Content = _localizer.Get("TodaysFocus.Text"),
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
                        },
                        adaptiveLargeTemplate
                    }
                }
            };

            var tileNotification = _tileService.AdaptiveFactory.Create(adaptiveTileModel);

            tileNotification.ExpirationTime = DateTimeOffset.Now.Date.AddDays(1);
            _tileService.GetUpdaterForApplication().Update(tileNotification);
        }

        /// <summary>
        /// Creates a text placeholder.
        /// </summary>
        /// <returns>Returns a next adaptive text element with empty text.</returns>
        private static AdaptiveText CreateTextPlaceholder()
        {
            return new AdaptiveText() { Content = string.Empty };
        }
    }
}
