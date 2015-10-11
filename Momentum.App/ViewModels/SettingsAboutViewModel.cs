using Momentum.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPCore.Framework.Mvvm;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Momentum.App.ViewModels
{
    class SettingsAboutViewModel : ViewModelBase
    {
        private IImageService _imageService;

        public SettingsAboutViewModel()
        {
            _imageService = Injector.Get<IImageService>();
        }

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            base.OnNavigatedTo(parameter, mode, state);

            await LoadBackgroundImageAsync();
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
            }

            IsLoadingBackground = false;
        }

        /// <summary>
        /// Gets or sets the app background image.
        /// </summary>
        public ImageSource BackgroundImageSource { get { return _backgroundImageSource; } set { Set(ref _backgroundImageSource, value); } }
        private ImageSource _backgroundImageSource;

        /// <summary>
        /// Gets or sets whether the loading of the background image is in progress.
        /// </summary>
        public bool IsLoadingBackground { get { return _isLoadingBackground; } set { Set(ref _isLoadingBackground, value); } }
        private bool _isLoadingBackground;
    }
}
