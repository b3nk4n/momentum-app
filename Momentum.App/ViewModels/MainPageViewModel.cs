using Momentum.App.Views;
using System.Collections.Generic;
using UWPCore.Framework.Mvvm;
using UWPCore.Framework.Navigation;
using Windows.UI.Xaml.Navigation;

namespace Momentum.App.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets the command to navigate to the about page.
        /// </summary>
        public DelegateCommand NavigateAboutCommand { get { return _navigateAboutCommand ?? (_navigateAboutCommand = new DelegateCommand(ExecuteNavigateAbout)); } }
        DelegateCommand _navigateAboutCommand = default(DelegateCommand);
        private void ExecuteNavigateAbout()
        {
            NavigationService.Navigate(typeof(AboutPage));
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            base.OnNavigatedTo(parameter, mode, state);
        }

        public override void OnNavigatingFrom(NavigatingEventArgs args)
        {
            base.OnNavigatingFrom(args);
        }
    }
}
