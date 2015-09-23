using Momentum.Common;
using Momentum.Common.Models;
using System;
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
        private ISerializationService _serializationService;

        private Localizer _localizer = new Localizer("Momentum.Common");

        public TimedUpdaterTask()
        {
            _toastService = new ToastService();
            _serializationService = new DataContractSerializationService();
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            var lastestFocus = GetLatestFocus();

            // clear history to make sure that there are no multiple daily focus messages
            _toastService.ClearHistory();

            // only one popup per day
            if (lastestFocus.Timestamp.Date != DateTime.Now.Date)
            {
                if (!string.IsNullOrEmpty(lastestFocus.Message))
                    ResetTodaysFocusMessage(lastestFocus);

                // create toast message when there was no focus message for today
                var adaptiveToastModel = CreateToast();
                var xml = adaptiveToastModel.GetXmlDocument().ToString();
                var toastNotification = _toastService.AdaptiveFactory.Create(adaptiveToastModel);
                _toastService.Show(toastNotification);
            }

            deferral.Complete();
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
