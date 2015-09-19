using Momentum.App.Services;
using Momentum.App.Views;
using System.Collections.Generic;
using System.Threading.Tasks;
using UWPCore.Framework.Accounts;
using UWPCore.Framework.Mvvm;
using UWPCore.Framework.Navigation;
using Windows.Globalization;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System;
using Momentum.Common;

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

        /// <summary>
        /// Creates a MainPageViewModel instance.
        /// </summary>
        public MainPageViewModel()
        {
            _imageService = new BingImageService(ApplicationLanguages.Languages[0]);
            _quoteService = new QuoteService(ApplicationLanguages.Languages[0]);
            _userInfoService = new UserInfoService();
        }

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            base.OnNavigatedTo(parameter, mode, state);

            await LoadBackgroundImageAsync();
            await LoadQuoteAsync();
            await LoadUserNameAsync();
        }

        public override void OnNavigatingFrom(NavigatingEventArgs args)
        {
            base.OnNavigatingFrom(args);
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
                BackgroundImageSource = backgroundImageResult.ImageSource;
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

        private async Task LoadUserNameAsync()
        {
            var userNameFromSettings = AppSettings.UserName.Value;

            if (string.IsNullOrEmpty(userNameFromSettings))
            {
                UserName = await _userInfoService.GetFirstNameAsync();
            }
            else
            {
                UserName = userNameFromSettings;
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
            //NavigationService.Navigate(typeof(SettingsPage));
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
