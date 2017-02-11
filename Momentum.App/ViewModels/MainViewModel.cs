﻿using Momentum.App.Views;
using Momentum.Common;
using Momentum.Common.Models;
using Momentum.Common.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UWPCore.Framework.Accounts;
using UWPCore.Framework.Common;
using UWPCore.Framework.Data;
using UWPCore.Framework.Devices;
using UWPCore.Framework.Mvvm;
using UWPCore.Framework.Navigation;
using UWPCore.Framework.Speech;
using UWPCore.Framework.Store;
using UWPCore.Framework.UI;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Momentum.App.ViewModels
{
    /// <summary>
    /// Callback interface to notify the UI to play animations.
    /// </summary>
    public interface MainViewModelCallbacks
    {
        void NotifyImageLoaded();
        void NotifyQuoteLoaded();
    }

    /// <summary>
    /// The view model of the main page.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private const string PRO_VERSION = "DailyFocus_Pro";

        private IImageService _imageService;
        private IQuoteService _quoteService;
        private IUserInfoService _userInfoService;
        private ISerializationService _serializationService;
        private ITileUpdateService _tileUpdateService;
        private ISpeechService _speechService;
        private IPersonalizationService _personalizationService;
        private IDialogService _dialogService;
        private ILicenseService _licenseService;

        private Localizer _localizer = new Localizer("Momentum.Common");

        private TypedEventHandler<ApplicationData, object> dataChangedHandler = null;

        private DispatcherTimer _timeUpdater;

        private MainViewModelCallbacks _callbacks;

        /// <summary>
        /// Creates a MainPageViewModel instance.
        /// </summary>
        public MainViewModel(MainViewModelCallbacks callbacks)
        {
            _callbacks = callbacks;

            _imageService = Injector.Get<IImageService>();
            _quoteService = Injector.Get<IQuoteService>();
            _userInfoService = Injector.Get<IUserInfoService>();
            _serializationService = Injector.Get<ISerializationService>();
            _tileUpdateService = Injector.Get<ITileUpdateService>();
            _speechService = Injector.Get<ISpeechService>();
            _personalizationService = Injector.Get<IPersonalizationService>();
            _dialogService = Injector.Get<IDialogService>();
            _licenseService = Injector.Get<ILicenseService>();
        }

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            base.OnNavigatedTo(parameter, mode, state);

            StartCurrentTimeUpdater();

            UpdateTodaysFocusFromSettings();

            dataChangedHandler = new TypedEventHandler<ApplicationData, object>(DataChangedHandler);
            ApplicationData.Current.DataChanged += dataChangedHandler;

            await LoadUserNameAsync();

            await LoadBackgroundImageAsync();
            await LoadQuoteAsync();
        }

        public async override void OnResume()
        {
            base.OnResume();

            StartCurrentTimeUpdater();

            // reload new data on resume
            UpdateTodaysFocusFromSettings();
            await LoadBackgroundImageAsync();
            await LoadQuoteAsync();
        }

        /// <summary>
        /// Starts the current time updater and updates the current time that is displayed.
        /// </summary>
        private void StartCurrentTimeUpdater()
        {
            if (_timeUpdater == null)
            {
                _timeUpdater = new DispatcherTimer();
                _timeUpdater.Interval = TimeSpan.FromSeconds(60.1 - DateTimeOffset.Now.Second);
                _timeUpdater.Tick += (s, e) =>
                {
                    _timeUpdater.Interval = TimeSpan.FromSeconds(60.1 - DateTimeOffset.Now.Second);
                    CurrentTime = DateTimeOffset.Now;
                };
                _timeUpdater.Start();
            }
            CurrentTime = DateTimeOffset.Now;
        }

        /// <summary>
        /// Stops the time updater.
        /// </summary>
        private void StopCurrentTimeUpdater()
        {
            if (_timeUpdater != null)
            {
                _timeUpdater.Stop();
                _timeUpdater = null;
            }
        }

        public async override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            await base.OnNavigatedFromAsync(state, suspending);

            ApplicationData.Current.DataChanged -= dataChangedHandler;
            dataChangedHandler = null;

            var todaysFocusModel = new TodaysFocusModel()
            {
                Message = TodaysFocus,
                Timestamp = _todaysFocusTimestamp
            };

            // save when todays focus message has changed
            if (_oldTodaysFocus != TodaysFocus && TodaysFocus != null)
            {
                _todaysFocusTimestamp = DateTime.Now;
                todaysFocusModel.Timestamp = _todaysFocusTimestamp;

                // save
                AppSettings.TodaysFocusJson.Value = _serializationService.SerializeJson(todaysFocusModel);
            }

            // update the live tile always, because it could have been changed on a different machine meanwhile
            await _tileUpdateService.UpdateLiveTile(todaysFocusModel);

            StopCurrentTimeUpdater();
        }

        private async void DataChangedHandler(ApplicationData appData, object o)
        {
            // dataChangeHandler may be invoked on a background thread, so use the Dispatcher to invoke the UI-related code on the UI thread
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UserName = AppSettings.UserName.Value;
                UpdateTodaysFocusFromSettings();
            });
        }

        /// <summary>
        /// Loads the background image.
        /// </summary>
        /// <returns>The async task to wait for.</returns>
        private async Task LoadBackgroundImageAsync()
        {
            IsLoading = true;

            var backgroundImageResult = await _imageService.LoadImageAsync();

            if (backgroundImageResult != null)
            {
                BackgroundImageSource = new BitmapImage(new Uri(backgroundImageResult.ImagePath));
                BackgroundCopyright = ShortifyText(backgroundImageResult.Copryright);

                _callbacks.NotifyImageLoaded();
            }

            IsLoading = false;
        }

        /// <summary>
        /// Loads the quote.
        /// </summary>
        /// <returns>The async task to wait for.</returns>
        private async Task LoadQuoteAsync()
        {
            var quoteData = await _quoteService.LoadQuoteAsync();

            if (quoteData != null)
            {
                QuoteText = MultilinifyText(quoteData.quote);
                QuoteAuthor = ShortifyText(quoteData.author);

                _callbacks.NotifyQuoteLoaded();
            }
        }

        /// <summary>
        /// Converts a text into mulitple lines.
        /// </summary>
        /// <param name="text">The text to edit.</param>
        /// <returns>The edited text.</returns>
        private string MultilinifyText(string text)
        {
            try // try catch just to make sure the app will not crash here ... :)
            {
                // should fit in one line
                if (text.Length < 40)
                    return text;

                var sepIndexes = text.AllIndexesOf(new[] { ',', '.', ';' });

                if (sepIndexes.Length < 2)
                    return text;

                int center = text.Length / 2;

                // find separator which is the closes to the center
                int currentIndex = -1;
                int currentDiff = int.MaxValue;

                for (int i = 0; i < sepIndexes.Length; ++i)
                {
                    int diff = Math.Abs(center - sepIndexes[i]);
                    if (currentDiff > diff)
                    {
                        currentIndex = i;
                        currentDiff = diff;
                    }
                }

                // paranoia protector :)
                if (currentIndex == -1)
                    return text;

                var sb = new StringBuilder();

                string part1 = text.Substring(0, sepIndexes[currentIndex] + 1).TrimEnd();
                string part2 = text.Substring(sepIndexes[currentIndex] + 1).TrimStart();
                sb.AppendLine(part1);
                sb.Append(part2);
                return sb.ToString();
            }
            catch (Exception)
            {
                return text;
            }
        }

        /// <summary>
        /// Tries to make the text shorter.
        /// </summary>
        /// <param name="text">The text to edit.</param>
        /// <returns>The edited text.</returns>
        private string ShortifyText(string text)
        {
            if (text.Length < 30)
                return text;

            int trimIndex = text.IndexOfAny(new[] { ')', ']' });

            if (trimIndex == -1)
            {
                trimIndex = text.IndexOfAny(new[] { ',', '.', ';' });
                --trimIndex; // to remove the separator
            }

            if (trimIndex > 0)
            {
                return text.Substring(0, trimIndex + 1);
            }

            return text;
        }

        /// <summary>
        /// Loads the roaming user name.
        /// </summary>
        /// <returns>The async task to wait for.</returns>
        private async Task LoadUserNameAsync()
        {
            var userNameFromSettings = AppSettings.UserName.Value;

            if (string.IsNullOrEmpty(userNameFromSettings))
            {
                var name = await _userInfoService.GetFirstNameAsync();

                if (string.IsNullOrEmpty(name))
                {
                    UserName = _localizer.Get("DefaultUserName");
                }
                else
                {
                    UserName = name;
                }  

                AppSettings.UserName.Value = UserName;
            }
            else
            {
                UserName = userNameFromSettings;
            }
        }

        /// <summary>
        /// Updates the todays focus from app settings.
        /// </summary>
        private void UpdateTodaysFocusFromSettings()
        {
            var todaysFocusJson = AppSettings.TodaysFocusJson.Value;
            if (!string.IsNullOrEmpty(todaysFocusJson))
            {
                var todaysFocusModel = _serializationService.DeserializeJson<TodaysFocusModel>(todaysFocusJson);
                if (todaysFocusModel != null)
                {
                    _todaysFocusTimestamp = todaysFocusModel.Timestamp;

                    if (!AppUtils.NeedsUpdate(_todaysFocusTimestamp))
                    {
                        TodaysFocus = todaysFocusModel.Message;
                        _oldTodaysFocus = todaysFocusModel.Message;
                    }
                    else
                    {
                        TodaysFocus = string.Empty;
                        _oldTodaysFocus = string.Empty;
                    }
                }
            }
            else
            {
                // make old value Empty and NOT NULL, because the app would think tha data has changed and upload a null value!
                _oldTodaysFocus = string.Empty;
            }
        }

        /// <summary>
        /// Gets the command to navigate to the about page.
        /// </summary>
        public DelegateCommand NavigateAboutCommand { get { return _navigateAboutCommand ?? (_navigateAboutCommand = new DelegateCommand(ExecuteNavigateAbout)); } }
        DelegateCommand _navigateAboutCommand = default(DelegateCommand);
        private void ExecuteNavigateAbout()
        {
            NavigationService.Navigate(typeof(AboutPage));
        }

        /// <summary>
        /// Gets the command to navigate to the settings page.
        /// </summary>
        public DelegateCommand NavigateSettingsCommand { get { return _navigateSettingsCommand ?? (_navigateSettingsCommand = new DelegateCommand(ExecuteNavigateSettings)); } }
        DelegateCommand _navigateSettingsCommand = default(DelegateCommand);
        private void ExecuteNavigateSettings()
        {
            NavigationService.Navigate(typeof(SettingsPage));
        }

        /// <summary>
        /// Gets the command to set the lockscreen image.
        /// </summary>
        public DelegateCommand SetAsLockscreenCommand { get { return _setAsLockscreenCommand ?? (_setAsLockscreenCommand = new DelegateCommand(ExecuteSetAsLockscreen)); } }
        DelegateCommand _setAsLockscreenCommand = default(DelegateCommand);
        private async void ExecuteSetAsLockscreen()
        {
            IsLoading = true;

            var image = await _imageService.GetBackgroundAsFileAsync();

            bool result = false;

            if (image != null)
                result = await _personalizationService.SetLockScreenAsync(image);

            if (result)
            {
                // fake some progress
                await Task.Delay(1000);
            }
            else
            {
                await _dialogService.ShowAsync(_localizer.Get("DeviceNotSupportedDialog.Content"), _localizer.Get("DeviceNotSupportedDialog.Title"));
            }

            IsLoading = false;
        }

        /// <summary>
        /// Gets the command to set the wallpaper image.
        /// </summary>
        public DelegateCommand SetAsWallpaperCommand { get { return _setAsWallpaperCommand ?? (_setAsWallpaperCommand = new DelegateCommand(ExecuteSetAsWallpaper)); } }
        DelegateCommand _setAsWallpaperCommand = default(DelegateCommand);
        private async void ExecuteSetAsWallpaper()
        {
            IsLoading = true;

            var image = await _imageService.GetBackgroundAsFileAsync();

            bool result = false;

            if (image != null)
                result = await _personalizationService.SetWallpaperAsync(image);

            if (result)
            {
                // fake some progress
                await Task.Delay(1000);
            }
            else
            {
                await _dialogService.ShowAsync(_localizer.Get("DeviceNotSupportedDialog.Content"), _localizer.Get("DeviceNotSupportedDialog.Title"));
            }

            IsLoading = false;
        }

        /// <summary>
        /// Gets the command to read the quote.
        /// </summary>
        public DelegateCommand ReadQuoteCommand { get { return _readQuoteCommand ?? (_readQuoteCommand = new DelegateCommand(ExecuteReadQuote)); } }
        DelegateCommand _readQuoteCommand = default(DelegateCommand);
        private async void ExecuteReadQuote()
        {
            if (!string.IsNullOrWhiteSpace(QuoteText))
            {
                await _speechService.SpeakTextAsync(QuoteText);
            }
        }

        /// <summary>
        /// Gets the command to read the quote.
        /// </summary>
        public DelegateCommand PurchaseProVersionCommand { get { return _purchaseProVersionCommand ?? (_purchaseProVersionCommand = new DelegateCommand(ExecutePurchaseProVersion, CanExecutePurchaseProVersion)); } }
        DelegateCommand _purchaseProVersionCommand = default(DelegateCommand);
        private async void ExecutePurchaseProVersion()
        {
            var res = await _licenseService.RequestProductPurchaseAsync(PRO_VERSION);
            RaisePropertyChanged("IsAdVisible");
        }
        private bool CanExecutePurchaseProVersion()
        {
            return !_licenseService.IsProductActive(PRO_VERSION);
        }

        /// <summary>
        /// Gets whether the pro version is inactive and the adverts should be visible.
        /// </summary>
        public bool IsAdVisible {
            get
            {
                var isActive = _licenseService.IsProductActive(PRO_VERSION);
                return !isActive;
            }
        }

        /// <summary>
        /// Gets or sets the app background image.
        /// </summary>
        public ImageSource BackgroundImageSource { get { return _backgroundImageSource; } set { Set(ref _backgroundImageSource, value); } }
        private ImageSource _backgroundImageSource;

        /// <summary>
        /// Gets or sets the background image copyright.
        /// </summary>
        public string BackgroundCopyright { get { return _backgroundCopyright; } set { Set(ref _backgroundCopyright, value); } }
        private string _backgroundCopyright;

        /// <summary>
        /// Gets or sets whether the loading of the background image or setting it is in progress.
        /// </summary>
        public bool IsLoading { get { return _isLoading; } set { Set(ref _isLoading, value); } }
        private bool _isLoading;

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get { return _userName; } set { Set(ref _userName, value); } }
        private string _userName;

        /// <summary>
        /// Gets or sets the user todays focus message.
        /// </summary>
        public string TodaysFocus
        {
            get
            {
                return _todaysFocusMessage;
            }
            set
            {
                Set(ref _todaysFocusMessage, value);
            }
        }
        private string _todaysFocusMessage;
        private DateTime _todaysFocusTimestamp;
        private string _oldTodaysFocus;

        /// <summary>
        /// Gets or sets the quote text.
        /// </summary>
        public string QuoteText { get { return _quoteText; } set { Set(ref _quoteText, value); } }
        private string _quoteText;

        /// <summary>
        /// Gets or sets the quote text.
        /// </summary>
        public string QuoteAuthor { get { return _quoteAuthor; } set { Set(ref _quoteAuthor, value); } }
        private string _quoteAuthor;

        /// <summary>
        /// Gets or sets the current time.
        /// </summary>
        public DateTimeOffset CurrentTime { get { return _currentTime; } set { Set(ref _currentTime, value); } }
        private DateTimeOffset _currentTime;

        public string WelcomeStart
        {
            get
            {
                return AppUtils.GetWelcomeMessageStart();
            }
        }
    }
}
