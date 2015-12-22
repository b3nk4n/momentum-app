using Momentum.Common;
using Momentum.Common.Models;
using Momentum.Common.Modules;
using Momentum.Common.Services;
using System;
using UWPCore.Framework.Data;
using UWPCore.Framework.IoC;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace Momentum.Tasks
{
    public sealed class ToastNotificationSaveTask : IBackgroundTask
    {
        private ISerializationService _serializationService;
        private ITileUpdateService _tileUpdateService;

        public ToastNotificationSaveTask()
        {
            IInjector injector = Injector.Instance;
            injector.Init(new DefaultModule(), new ReleaseModule());
            _serializationService = injector.Get<ISerializationService>();
            _tileUpdateService = injector.Get<ITileUpdateService>();
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            // check for toast input
            var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
            if (details != null)
            {
                var userInput = (string)details.UserInput["message"];

                // only update when the user entered a value to ensure not override not a value of another device with empty text
                if (!string.IsNullOrWhiteSpace(userInput))
                {
                    var focusModel = new TodaysFocusModel()
                    {
                        Message = userInput,
                        Timestamp = DateTime.Now
                    };

                    AppSettings.TodaysFocusJson.Value = _serializationService.SerializeJson(focusModel);

                    // update tile
                    _tileUpdateService.UpdateLiveTile(focusModel);
                }
            }

            deferral.Complete();
        }
    }
}
