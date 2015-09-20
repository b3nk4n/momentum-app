using Momentum.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Momentum.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            UserNameTextBox.Text = AppSettings.UserName.Value;
        }

        // TODO: FIXME - not OnNavigatedFrom event was used here, because it is called AFTER the OnNavigatedTo event of the next page...
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            AppSettings.UserName.Value = UserNameTextBox.Text;
        }
    }
}
