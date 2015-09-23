using Momentum.Common;
using Momentum.Common.Models;
using System;
using UWPCore.Framework.Data;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace Momentum.Tasks
{
    public sealed class ToastNotificationSaveTask : IBackgroundTask
    {
        private ISerializationService _serializationService;

        public ToastNotificationSaveTask()
        {
            _serializationService = new DataContractSerializationService();
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            // check for toast input
            var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
            if (details != null)
            {
                var userInput = (string)details.UserInput["message"];

                var focusModel = new TodaysFocusModel()
                {
                    Message = userInput,
                    Timestamp = DateTime.Now
                };

                AppSettings.TodaysFocusJson.Value = _serializationService.SerializeJson(focusModel);
            }

            deferral.Complete();
        }
    }
}
