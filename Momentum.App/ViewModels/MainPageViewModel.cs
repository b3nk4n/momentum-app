using Momentum.App.Services;
using Momentum.App.Views;
using System.Collections.Generic;
using System.Threading.Tasks;
using UWPCore.Framework.Mvvm;
using UWPCore.Framework.Navigation;
using Windows.Globalization;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Momentum.App.ViewModels
{
    /// <summary>
    /// The view model of the main page.
    /// </summary>
    public class MainPageViewModel : ViewModelBase
    {
        private BingImageService _bingImageService;

        /// <summary>
        /// Creates a MainPageViewModel instance.
        /// </summary>
        public MainPageViewModel()
        {
            _bingImageService = new BingImageService(ApplicationLanguages.Languages[0]);
        }

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            base.OnNavigatedTo(parameter, mode, state);

            await LoadBackgroundImageAsync();
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
            var backgroundImageSource = await _bingImageService.LoadImageAsync();

            if (backgroundImageSource != null)
                BackgroundImageSource = backgroundImageSource;
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
        /// Gets or sets the app icon.
        /// </summary>
        public ImageSource BackgroundImageSource { get { return _backgroundImageSource; } set { Set(ref _backgroundImageSource, value); } }
        private ImageSource _backgroundImageSource;
    }
}
