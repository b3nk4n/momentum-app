using Microsoft.AdMediator.Core.Events;
using Microsoft.AdMediator.Core.Models;
using Momentum.App.ViewModels;
using System;
using UWPCore.Framework.Controls;
using UWPCore.Framework.Logging;
using UWPCore.Framework.Notifications;
using UWPCore.Framework.Tasks;
using Windows.ApplicationModel.Background;

namespace Momentum.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : UniversalPage, MainViewModelCallbacks
    {
        private static string BG_TASK_TIMED_NAME = "Momentum.TimedUpdaterTask";
        private static string BG_TASK_TOAST_NAME = "Momentum.ToastNotificationSaveTask";

        IBackgroundTaskService _backgroundTaskService;
        IToastService _toastService;

        public MainPage()
        {
            InitializeComponent();

            DataContext = new MainViewModel(this);

            _backgroundTaskService = Injector.Get<IBackgroundTaskService>();
            _toastService = Injector.Get<IToastService>();

            Loaded += (s, e) =>
            {
                RegisterBackgroundTask();

                // clear action center when app was launched 
                _toastService.ClearHistory();
            };

            ConfigureAdverts();
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

        void MainViewModelCallbacks.NotifyImageLoaded()
        {
            ShowBackgroundImage.Begin();
            StartupAnimation.Begin();
        }

        void MainViewModelCallbacks.NotifyQuoteLoaded()
        {
            ShowQuote.Begin();
        }

        private void ClearFocusClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            FocusTextBox.Text = string.Empty;
        }

        private void QuoteTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;

            if (viewModel != null)
            {
                viewModel.ReadQuoteCommand.Execute(null);
            }
        }

        /// <summary>
        /// Configures the adverts.
        /// </summary>
        private void ConfigureAdverts()
        {
            AdMediator.AdSdkError += AdMediator_AdError;
            AdMediator.AdMediatorFilled += AdMediator_AdFilled;
            AdMediator.AdMediatorError += AdMediator_AdMediatorError;
            AdMediator.AdSdkEvent += AdMediator_AdSdkEvent;

            AdMediator.AdSdkOptionalParameters[AdSdkNames.MicrosoftAdvertising]["HorizontalAlignment"] = Windows.UI.Xaml.HorizontalAlignment.Center;
            AdMediator.AdSdkOptionalParameters[AdSdkNames.MicrosoftAdvertising]["VerticalAlignment"] = Windows.UI.Xaml.VerticalAlignment.Top;

            AdMediator.AdSdkOptionalParameters[AdSdkNames.AdDuplex]["HorizontalAlignment"] = Windows.UI.Xaml.HorizontalAlignment.Center;
            AdMediator.AdSdkOptionalParameters[AdSdkNames.AdDuplex]["VerticalAlignment"] = Windows.UI.Xaml.VerticalAlignment.Top;
        }

        void AdMediator_AdSdkEvent(object sender, AdSdkEventArgs e)
        {
            Logger.WriteLine("AdSdk event {0} by {1}", e.EventName, e.Name);
        }

        void AdMediator_AdMediatorError(object sender, AdMediatorFailedEventArgs e)
        {
            Logger.WriteLine("AdMediatorError:" + e.Error + " " + e.ErrorCode);
        }

        void AdMediator_AdFilled(object sender, AdSdkEventArgs e)
        {
            Logger.WriteLine("AdFilled:" + e.Name);
        }

        void AdMediator_AdError(object sender, AdFailedEventArgs e)
        {
            Logger.WriteLine("AdSdkError by {0} ErrorCode: {1} ErrorDescription: {2} Error: {3}", e.Name, e.ErrorCode, e.ErrorDescription, e.Error);
        }
    }
}
