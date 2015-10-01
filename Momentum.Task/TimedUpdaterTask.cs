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

namespace Momentum.Tasks
{
    /// <summary>
    /// Background task to update the tile and push a notification in the morning.
    /// </summary>
    public sealed class TimedUpdaterTask : IBackgroundTask
    {
        private IToastService _toastService;
        private ITileUpdateService _tileUpdateService;
        private ISerializationService _serializationService;

        private Localizer _localizer = new Localizer("Momentum.Common");

        public TimedUpdaterTask()
        {
            _toastService = new ToastService();
            _tileUpdateService = new TileUpdateService();
            _serializationService = new DataContractSerializationService();
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            var latestFocus = GetLatestFocus();

            UpdateToasts(latestFocus);

            await _tileUpdateService.UpdateLiveTile(latestFocus);

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
            if (AppUtils.NeedsUpdate(latestFocus.Timestamp))
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
                                    Content = string.Format("{0} {1}.", AppUtils.GetWelcomeMessageStart() , AppSettings.UserName.Value)
                                },
                                new AdaptiveText()
                                {
                                    Content = "It's me again..." // TODO: which text here?
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
                            Content = _localizer.Get("Save"),
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
