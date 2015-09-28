using Momentum.Common;
using Momentum.Common.Models;
using Momentum.Common.Services;
using System;
using System.Threading.Tasks;
using UWPCore.Framework.Common;
using UWPCore.Framework.Data;
using UWPCore.Framework.Notifications;
using UWPCore.Framework.Notifications.Models;
using Windows.ApplicationModel.Background;
using Windows.Globalization;

namespace Momentum.Tasks
{
    /// <summary>
    /// Background task to update the tile and push a notification in the morning.
    /// </summary>
    public sealed class TimedUpdaterTask : IBackgroundTask
    {
        private IToastService _toastService;
        private ITileService _tileService;
        private ISerializationService _serializationService;
        private IImageService _imageService;

        private Localizer _localizer = new Localizer("Momentum.Common");

        public TimedUpdaterTask()
        {
            _toastService = new ToastService();
            _tileService = new TileService();
            _serializationService = new DataContractSerializationService();
            _imageService = new BingImageService(ApplicationLanguages.Languages[0]);
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            var latestFocus = GetLatestFocus();

            UpdateToasts(latestFocus);

            await UpdateLiveTile(latestFocus);

            deferral.Complete();
        }

        /// <summary>
        /// Updates the action center and the toast notification messages.
        /// </summary>
        /// <param name="latestFocus">The latest focus message.</param>
        private void UpdateToasts(TodaysFocusModel latestFocus)
        {
            // clear history to make sure that there are no multiple daily focus messages
            _toastService.ClearHistory();

            // only one (successfull) popup per day
            if (AppHelpers.NeedsUpdate(latestFocus.Timestamp))
            {
                // reset focus message
                if (!string.IsNullOrEmpty(latestFocus.Message))
                    ResetTodaysFocusMessage(latestFocus);

                // create toast message
                var adaptiveToastModel = CreateToast();
                var toastNotification = _toastService.AdaptiveFactory.Create(adaptiveToastModel);
                _toastService.Show(toastNotification);
            }
        }

        /// <summary>
        /// Updates the tile notifications.
        /// </summary>
        /// <param name="latestFocus">The latest focus message.</param>
        private async Task UpdateLiveTile(TodaysFocusModel latestFocus)
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

        /// <summary>
        /// Gets the lastest focus from the roaming settings.
        /// </summary>
        /// <returns>Returns the latest focus data.</returns>
        private TodaysFocusModel GetLatestFocus()
        {
            var focusJson = AppSettings.TodaysFocusJson.Value;
            return _serializationService.DeserializeJson<TodaysFocusModel>(focusJson);
        }

        /// <summary>
        /// Resets the todays focus message, because it is expired.
        /// </summary>
        private void ResetTodaysFocusMessage(TodaysFocusModel lastestFocus)
        {
            lastestFocus.Message = string.Empty;
            AppSettings.TodaysFocusJson.Value = _serializationService.SerializeJson(lastestFocus);
        }

        /// <summary>
        /// Creates the toast notification.
        /// </summary>
        /// <returns>The created toast notification.</returns>
        private AdaptiveToastModel CreateToast()
        {
            return new AdaptiveToastModel()
            {
                Visual = new AdaptiveVisual()
                {
                    Bindings = {
                        new AdaptiveBinding()
                        {
                            Branding = VisualBranding.NameAndLogo,
                            Template = VisualTemplate.ToastGeneric,
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Content = string.Format("Good morning, {0}.", AppSettings.UserName.Value)
                                },
                                new AdaptiveText()
                                {
                                    Content = "It's me again..."
                                }
                            }
                        }
                    }
                },
                Actions = new AdaptiveActions()
                {
                    Children =
                    {
                        new AdaptiveInput()
                        {
                            Type = InputType.Text,
                            PlaceHolderContent = _localizer.Get("WhatsYourFocus.Text"),
                            Id = "message",
                        },
                        new AdaptiveAction()
                        {
                            ActivationType = ToastActivationType.Background,
                            Content = "Save",
                            Arguments = "save",
                            HintInputId = "message",
                            ImageUri = "/Assets/Images/ok.png"
                        }
                    }
                }
            };
        }
    }
}
