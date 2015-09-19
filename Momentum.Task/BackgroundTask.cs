using System;
using UWPCore.Framework.Logging;
using UWPCore.Framework.Notifications;
using UWPCore.Framework.Notifications.Models;
using Windows.ApplicationModel.Background;

namespace Momentum.Tasks
{
    /// <summary>
    /// Background task to update the tile and push a notification in the morning.
    /// </summary>
    public sealed class BackgroundTask : IBackgroundTask
    {
        private IToastService _toastService;

        public BackgroundTask()
        {
            _toastService = new ToastService();
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            Logger.WriteLine("BG THREAD");

            var adaptiveToastModel = CreateToast();
            var toastNotification = _toastService.AdaptiveFactory.Create(adaptiveToastModel);
            _toastService.Show(toastNotification);

            deferral.Complete();
        }

        /// <summary>
        /// Creates the toast notification.
        /// </summary>
        /// <returns>The created toast notification.</returns>
        private static AdaptiveToastModel CreateToast()
        {
            return new AdaptiveToastModel()
            {
                Visual = new AdaptiveVisual()
                {
                    Bindings = {
                        new AdaptiveBinding()
                        {
                            DisplayName = "Display name",
                            Branding = VisualBranding.NameAndLogo,
                            Template = VisualTemplate.ToastGeneric,
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Content = "Good morning, Benny."
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
                            PlaceHolderContent = "What is your focus for today?",
                            Id = "focus",
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
