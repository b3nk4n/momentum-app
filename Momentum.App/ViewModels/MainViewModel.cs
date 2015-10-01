using Momentum.App.Views;
using Momentum.Common;
using Momentum.Common.Models;
using Momentum.Common.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UWPCore.Framework.Accounts;
using UWPCore.Framework.Common;
using UWPCore.Framework.Data;
using UWPCore.Framework.Mvvm;
using UWPCore.Framework.Navigation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Globalization;
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
        private IImageService _imageService;
        private IQuoteService _quoteService;
        private IUserInfoService _userInfoService;
        private ISerializationService _serializationService;
        private ITileUpdateService _tileUpdateService;

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

            _imageService = new BingImageService(ApplicationLanguages.Languages[0]);
            _quoteService = new QuoteService(ApplicationLanguages.Languages[0]);
            _userInfoService = new UserInfoService();
            _serializationService = new DataContractSerializationService();
            _tileUpdateService = new TileUpdateService();
        }

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            base.OnNavigatedTo(parameter, mode, state);

            StartCurrentTimeUpdater();

            UpdateTodaysFocusFromSettings();
            UpdateWhatsYourFocus();

            dataChangedHandler = new TypedEventHandler<ApplicationData, object>(DataChangedHandler);
            ApplicationData.Current.DataChanged += dataChangedHandler;

            await LoadUserNameAsync();

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
            if (_oldTodaysFocus != TodaysFocus)
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
                UpdateWhatsYourFocus();
            });
        }

        /// <summary>
        /// Loads the background image.
        /// </summary>
        /// <returns>The async task to wait for.</returns>
        private async Task LoadBackgroundImageAsync()
        {
            IsLoadingBackground = true;

            var backgroundImageResult = await _imageService.LoadImageAsync();

            if (backgroundImageResult != null)
            {
                BackgroundImageSource = new BitmapImage(new Uri(backgroundImageResult.ImagePath));
                BackgroundCopyright = backgroundImageResult.Copryright;

                _callbacks.NotifyImageLoaded();
            }

            IsLoadingBackground = false;
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
                QuoteText = quoteData.quote;
                QuoteAuthor = quoteData.author;

                _callbacks.NotifyQuoteLoaded();
            }
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
                    TodaysFocus = todaysFocusModel.Message;
                    _oldTodaysFocus = todaysFocusModel.Message;
                }
            }
        }

        /// <summary>
        /// Updates the "Whats your focus" text label depending on whether the textbox is empty or not. 
        /// </summary>
        private void UpdateWhatsYourFocus()
        {
            RaisePropertyChanged("WhatsYourFocus");
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
        /// Gets or sets whether the loading of the background image is in progress.
        /// </summary>
        public bool IsLoadingBackground { get { return _isLoadingBackground; } set { Set(ref _isLoadingBackground, value); } }
        private bool _isLoadingBackground;

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
                UpdateWhatsYourFocus();
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

        /// <summary>
        /// Gets or sets the whats your focus title text, which depends on whether the textbox is empty or not.
        /// </summary>
        public string WhatsYourFocus
        {
            get
            {
                if (string.IsNullOrEmpty(TodaysFocus))
                {
                    return _localizer.Get("WhatsYourFocus.Text");
                }
                else
                {
                    return _localizer.Get("TodaysFocus.Text");
                }
            }
        }

        public string WelcomeStart
        {
            get
            {
                return AppUtils.GetWelcomeMessageStart();
            }
        }
    }
}
