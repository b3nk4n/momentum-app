using System;
using UWPCore.Framework.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Momentum.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static string BG_TASK_TIMED_NAME = "Momentum.TimedUpdaterTask";
        private static string BG_TASK_TOAST_NAME = "Momentum.ToastNotificationSaveTask";

        IBackgroundTaskService _backgroundTaskService;

        public MainPage()
        {
            InitializeComponent();

            _backgroundTaskService = new BackgroundTaskService();

            Loaded += (s, e) =>
            {
                RegisterBackgroundTask();
            };
        }

        /// <summary>
        /// Registers the background task.
        /// </summary>
        private async void RegisterBackgroundTask()
        {
            if (_backgroundTaskService.RegistrationExists(BG_TASK_TIMED_NAME))
                return;

            if (await _backgroundTaskService.RequestAccessAsync())
            {
                _backgroundTaskService.Register(BG_TASK_TIMED_NAME, "Momentum.Tasks.TimedUpdaterTask", new TimeTrigger(60, false));
                _backgroundTaskService.Register(BG_TASK_TOAST_NAME, "Momentum.Tasks.ToastNotificationSaveTask", new ToastNotificationActionTrigger());
            }
                
        }
    }
}
