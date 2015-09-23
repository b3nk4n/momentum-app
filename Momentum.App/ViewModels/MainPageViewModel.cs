using Momentum.App.Views;
using Momentum.Common;
using Momentum.Common.Models;
using Momentum.Common.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UWPCore.Framework.Accounts;
using UWPCore.Framework.Data;
using UWPCore.Framework.Mvvm;
using UWPCore.Framework.Navigation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Globalization;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Momentum.App.ViewModels
{
    /// <summary>
    /// The view model of the main page.
    /// </summary>
    public class MainPageViewModel : ViewModelBase
    {
        private IImageService _imageService;
        private IQuoteService _quoteService;
        private IUserInfoService _userInfoService;
        private ISerializationService _serializationService;

        TypedEventHandler<ApplicationData, object> dataChangedHandler = null;

        /// <summary>
        /// Creates a MainPageViewModel instance.
        /// </summary>
        public MainPageViewModel()
        {
            _imageService = new BingImageService(ApplicationLanguages.Languages[0]);
            _quoteService = new QuoteService(ApplicationLanguages.Languages[0]);
            _userInfoService = new UserInfoService();
            _serializationService = new DataContractSerializationService();
        }

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            base.OnNavigatedTo(parameter, mode, state);

            UpdateTodaysFocusFromSettings();

            dataChangedHandler = new TypedEventHandler<ApplicationData, object>(DataChangedHandler);
            ApplicationData.Current.DataChanged += dataChangedHandler;

            await LoadBackgroundImageAsync();
            await LoadQuoteAsync();
            await LoadUserNameAsync();
        }

        // TODO: FIXME - currently it is not garanteed that this event is called when the app is terminated/suspended... (save it directly or adjust the FW?)
        public override void OnNavigatingFrom(NavigatingEventArgs args)
        {
            base.OnNavigatingFrom(args);

            ApplicationData.Current.DataChanged -= dataChangedHandler;
            dataChangedHandler = null;

            // save when todays focus message has changed
            if (_oldTodaysFocus != TodaysFocus)
            {
                _todaysFocusTimestamp = DateTime.Now;
                var todaysFocusModel = new TodaysFocusModel()
                {
                    Message = TodaysFocus,
                    Timestamp = _todaysFocusTimestamp
                };
                AppSettings.TodaysFocusJson.Value = _serializationService.SerializeJson(todaysFocusModel);
            }
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
            IsLoadingBackground = true;

            var backgroundImageResult = await _imageService.LoadImageAsync();

            if (backgroundImageResult != null)
            {
                BackgroundImageSource = new BitmapImage(new Uri(backgroundImageResult.ImagePath));
                BackgroundCopyright = backgroundImageResult.Copryright;
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
                    var loaded = new ResourceLoader();
                    UserName = loaded.GetString("DefaultUserName");
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
        public string TodaysFocus { get { return _todaysFocusMessage; } set { Set(ref _todaysFocusMessage, value); } }
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
    }
}
