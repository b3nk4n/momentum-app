using System;
using UWPCore.Framework.Controls;
using UWPCore.Framework.Notifications;
using UWPCore.Framework.Tasks;
using Windows.ApplicationModel.Background;

namespace Momentum.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : UniversalPage
    {
        private static string BG_TASK_TIMED_NAME = "Momentum.TimedUpdaterTask";
        private static string BG_TASK_TOAST_NAME = "Momentum.ToastNotificationSaveTask";

        IBackgroundTaskService _backgroundTaskService;

        IToastService _toastService;

        public MainPage()
        {
            InitializeComponent();

            _backgroundTaskService = new BackgroundTaskService();
            _toastService = new ToastService();

            Loaded += (s, e) =>
            {
                RegisterBackgroundTask();

                // clear action center when app was launched 
                _toastService.ClearHistory();
            };
        }

        /// <summary>
        /// (Re)registers the background task.
        /// </summary>
        private async void RegisterBackgroundTask()
        {
            // unregister previous one, to ensure the latest version is running
            if (_backgroundTaskService.RegistrationExists(BG_TASK_TIMED_NAME))
                _backgroundTaskService.Unregister(BG_TASK_TIMED_NAME);

            if (await _backgroundTaskService.RequestAccessAsync())
            {
                _backgroundTaskService.Register(BG_TASK_TIMED_NAME, "Momentum.Tasks.TimedUpdaterTask", new TimeTrigger(60, false));
                _backgroundTaskService.Register(BG_TASK_TOAST_NAME, "Momentum.Tasks.ToastNotificationSaveTask", new ToastNotificationActionTrigger());
            }     
        }

        private void NameDoulbeTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(SettingsPage), SettingsPage.PARAM_CHANGE_NAME);
        }
    }
}
