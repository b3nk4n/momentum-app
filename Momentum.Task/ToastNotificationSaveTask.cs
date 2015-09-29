﻿using Momentum.Common;
using Momentum.Common.Models;
using Momentum.Common.Services;
using System;
using UWPCore.Framework.Data;
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
            _serializationService = new DataContractSerializationService();
            _tileUpdateService = new TileUpdateService();
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

                // update tile
                _tileUpdateService.UpdateLiveTile(focusModel);
            }

            deferral.Complete();
        }
    }
}
