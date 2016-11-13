using Momentum.App.ViewModels;
using System;
using UWPCore.Framework.Controls;
using UWPCore.Framework.Devices;
using UWPCore.Framework.Notifications;
using UWPCore.Framework.Tasks;
using Windows.ApplicationModel.Background;
using Microsoft.Advertising.WinRT.UI;
using Windows.System.Profile;
using Windows.UI.Xaml;

namespace Momentum.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : UniversalPage, MainViewModelCallbacks
    {
        private static string BG_TASK_TIMED_NAME = "Momentum.TimedUpdaterTask";
        private static string BG_TASK_TOAST_NAME = "Momentum.ToastNotificationSaveTask";

        private IBackgroundTaskService _backgroundTaskService;
        private IToastService _toastService;
        private IDeviceInfoService _deviceInfoService;

        public MainPage()
        {
            InitializeComponent();

            ConfigureAdverts();

            DataContext = new MainViewModel(this);

            _backgroundTaskService = Injector.Get<IBackgroundTaskService>();
            _toastService = Injector.Get<IToastService>();
            _deviceInfoService = Injector.Get <IDeviceInfoService>();

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
                _backgroundTaskService.Register(BG_TASK_TIMED_NAME, "Momentum.Tasks.TimedUpdaterTask", new TimeTrigger(60, false), new SystemCondition(SystemConditionType.InternetAvailable));
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

        #region Adverts

        private const int AD_WIDTH = 320;
        private const int AD_HEIGHT = 50;
        private const int HOUSE_AD_WEIGHT = 5; // 5% AdHouse ads
        private const int AD_REFRESH_SECONDS = 35;
        private const int MAX_ERRORS_PER_REFRESH = 3;
        private const string WAPPLICATIONID = "891da8fa-fe3b-4aab-b9f5-8da90fceb155";
        private const string WADUNITID_PAID = "251980";
        private const string WADUNITID_HOUSE = "252004";
        private const string MAPPLICATIONID = "574b9a13-5c75-4ef8-9776-6f50d7734a7c";
        private const string MADUNITID_PAID = "251977";
        private const string MADUNITID_HOUSE = "252008";
        private const string ADDUPLEX_APPKEY = "4e7ab990-2cf9-4a79-8a35-0b8e1f4a671a";
        private const string ADDUPLEX_ADUNIT = "172239";

        // Dispatch timer to fire at each ad refresh interval.
        private DispatcherTimer myAdRefreshTimer = new DispatcherTimer();

        // Global variables used for mediation decisions.
        private Random randomGenerator = new Random();
        private int errorCountCurrentRefresh = 0;  // Prevents infinite redirects.
        private int adDuplexWeight = 0;            // Will be set by GetAdDuplexWeight().

        // Microsoft and AdDuplex controls for banner ads.
        private AdControl myMicrosoftBanner = null;
        private AdDuplex.AdControl myAdDuplexBanner = null;

        // Application ID and ad unit ID values for Microsoft advertising. By default,
        // assign these to non-mobile ad unit info.
        private string myMicrosoftAppId = WAPPLICATIONID;
        private string myMicrosoftPaidUnitId = WADUNITID_PAID;
        private string myMicrosoftHouseUnitId = WADUNITID_HOUSE;

        public void ConfigureAdverts()
        {
            myAdGrid.Width = AD_WIDTH;
            myAdGrid.Height = AD_HEIGHT;
            adDuplexWeight = 0;
            RefreshBanner();

            // Start the timer to refresh the banner at the desired interval.
            myAdRefreshTimer.Interval = new TimeSpan(0, 0, AD_REFRESH_SECONDS);
            myAdRefreshTimer.Tick += myAdRefreshTimer_Tick;
            myAdRefreshTimer.Start();

            // For mobile device families, use the mobile ad unit info.
            if ("Windows.Mobile" == AnalyticsInfo.VersionInfo.DeviceFamily)
            {
                myMicrosoftAppId = MAPPLICATIONID;
                myMicrosoftPaidUnitId = MADUNITID_PAID;
                myMicrosoftHouseUnitId = MADUNITID_HOUSE;
            }
        }

        private void ActivateMicrosoftBanner()
        {
            // Return if you hit the error limit for this refresh interval.
            if (errorCountCurrentRefresh >= MAX_ERRORS_PER_REFRESH)
            {
                myAdGrid.Visibility = Visibility.Collapsed;
                return;
            }

            // Use random number generator and house ads weight to determine whether
            // to use paid ads or house ads. Paid is the default. You could alternatively
            // write a method similar to GetAdDuplexWeight and override by region.
            string myAdUnit = myMicrosoftPaidUnitId;
            int houseWeight = HOUSE_AD_WEIGHT;
            int randomInt = randomGenerator.Next(0, 100);
            if (randomInt < houseWeight)
            {
                myAdUnit = myMicrosoftHouseUnitId;
            }

            // Hide the AdDuplex control if it is showing.
            if (null != myAdDuplexBanner)
            {
                myAdDuplexBanner.Visibility = Visibility.Collapsed;
                myAdGrid.Children.Remove(myAdDuplexBanner);
                myAdDuplexBanner = null;
            }

            // Initialize or display the Microsoft control.
            if (null == myMicrosoftBanner)
            {
                myMicrosoftBanner = new AdControl();
                myMicrosoftBanner.ApplicationId = myMicrosoftAppId;
                myMicrosoftBanner.AdUnitId = myAdUnit;
                myMicrosoftBanner.Width = AD_WIDTH;
                myMicrosoftBanner.Height = AD_HEIGHT;
                myMicrosoftBanner.IsAutoRefreshEnabled = false;

                myMicrosoftBanner.AdRefreshed += myMicrosoftBanner_AdRefreshed;
                myMicrosoftBanner.ErrorOccurred += myMicrosoftBanner_ErrorOccurred;

                myAdGrid.Children.Add(myMicrosoftBanner);
            }
            else
            {
                myMicrosoftBanner.AdUnitId = myAdUnit;
                myMicrosoftBanner.Visibility = Visibility.Visible;
                myMicrosoftBanner.Refresh();
            }
        }

        private void ActivateAdDuplexBanner()
        {
            // Return if you hit the error limit for this refresh interval.
            if (errorCountCurrentRefresh >= MAX_ERRORS_PER_REFRESH)
            {
                myAdGrid.Visibility = Visibility.Collapsed;
                return;
            }

            // Hide the Microsoft control if it is showing.
            if (null != myMicrosoftBanner)
            {
                myMicrosoftBanner.Visibility = Visibility.Collapsed;
                myAdGrid.Children.Remove(myMicrosoftBanner);
                myMicrosoftBanner = null;
            }

            // Initialize or display the AdDuplex control.
            if (null == myAdDuplexBanner)
            {
                myAdDuplexBanner = new AdDuplex.AdControl();
                myAdDuplexBanner.AppKey = ADDUPLEX_APPKEY;
                myAdDuplexBanner.AdUnitId = ADDUPLEX_ADUNIT;
                myAdDuplexBanner.Width = AD_WIDTH;
                myAdDuplexBanner.Height = AD_HEIGHT;
                myAdDuplexBanner.RefreshInterval = AD_REFRESH_SECONDS;

                myAdDuplexBanner.AdLoaded += myAdDuplexBanner_AdLoaded;
                myAdDuplexBanner.AdCovered += myAdDuplexBanner_AdCovered;
                myAdDuplexBanner.AdLoadingError += myAdDuplexBanner_AdLoadingError;
                myAdDuplexBanner.NoAd += myAdDuplexBanner_NoAd;

                myAdGrid.Children.Add(myAdDuplexBanner);
            }
            myAdDuplexBanner.Visibility = Visibility.Visible;
        }

        private void myAdRefreshTimer_Tick(object sender, object e)
        {
            RefreshBanner();
        }

        private void RefreshBanner()
        {
            // Reset the error counter for this refresh interval and
            // make sure the ad grid is visible.
            errorCountCurrentRefresh = 0;
            myAdGrid.Visibility = Visibility.Visible;

            ActivateMicrosoftBanner();
        }

        private void myMicrosoftBanner_AdRefreshed(object sender, RoutedEventArgs e)
        {
            // Add your code here as necessary.
        }

        private void myMicrosoftBanner_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            errorCountCurrentRefresh++;
            ActivateAdDuplexBanner();
        }

        private void myAdDuplexBanner_AdLoaded(object sender, AdDuplex.Banners.Models.BannerAdLoadedEventArgs e)
        {
            // Add your code here as necessary.
        }

        private void myAdDuplexBanner_NoAd(object sender, AdDuplex.Common.Models.NoAdEventArgs e)
        {
            errorCountCurrentRefresh++;
            ActivateMicrosoftBanner();
        }

        private void myAdDuplexBanner_AdLoadingError(object sender, AdDuplex.Common.Models.AdLoadingErrorEventArgs e)
        {
            errorCountCurrentRefresh++;
            ActivateMicrosoftBanner();
        }

        private void myAdDuplexBanner_AdCovered(object sender, AdDuplex.Banners.Core.AdCoveredEventArgs e)
        {
            errorCountCurrentRefresh++;
            ActivateMicrosoftBanner();
        }

        #endregion
    }
}
