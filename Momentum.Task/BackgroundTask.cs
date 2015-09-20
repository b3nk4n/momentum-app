using Momentum.Common;
using Momentum.Common.Models;
using System;
using UWPCore.Framework.Common;
using UWPCore.Framework.Data;
using UWPCore.Framework.Notifications;
using UWPCore.Framework.Notifications.Models;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace Momentum.Tasks
{
    /// <summary>
    /// Background task to update the tile and push a notification in the morning.
    /// </summary>
    public sealed class BackgroundTask : IBackgroundTask
    {
        private IToastService _toastService;
        private ISerializationService _serializationService;

        private Localizer _localizer = new Localizer("Momentum.Common");

        public BackgroundTask()
        {
            _toastService = new ToastService();
            _serializationService = new DataContractSerializationService();
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            // check for toast input
            var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
            if (details != null)
            {
                string arguments = details.Argument;
                var userInput = (string)details.UserInput["message"];

                var focusModel = new TodaysFocusModel()
                {
                    Message = userInput,
                    Timestamp = DateTime.Now
                };

                AppSettings.TodaysFocusJson.Value = _serializationService.SerializeJson(focusModel);

                deferral.Complete();
                return;
            }

            var lastestFocus = GetLatestFocus();

            // only one popup per day
            if (lastestFocus.Timestamp.Date != DateTime.Now.Date)
            {
                // clear history to make sure that there are no multiple daily focus questions
                _toastService.ClearHistory();

                // create toast message
                var adaptiveToastModel = CreateToast();
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
                            Content = "OK",
                            Arguments = "ok",
                            HintInputId = "message"
                        }
                    }
                }
            };
        }
    }
}
