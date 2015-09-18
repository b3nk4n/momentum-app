using System;
using UWPCore.Framework.Notifications;
using UWPCore.Framework.Notifications.Models;
using Windows.ApplicationModel.Background;

namespace Momentum.Task
{
    /// <summary>
    /// Background task to update the tile and push a notification in the morning.
    /// </summary>
    public class BackgroundTask : IBackgroundTask
    {
        private IToastService _toastService;

        public BackgroundTask()
        {
            _toastService = new ToastService();
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
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
                            Template = VisualTemplate.ToastGeneric,
                            Children =
                            {
                                new AdaptiveImage()
                                {
                                    Alt = "Cap Reef Milky",
                                    Source = "msappx:///Assets/Images/CapReefMilky_EN-US.jpg",
                                    HintCrop = ImageHintCrop.Circle
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
                            Title = "Good morning, Benny."
                        }
                    }
                }
            };
        }
    }
}
