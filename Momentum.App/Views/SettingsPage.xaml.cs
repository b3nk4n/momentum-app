using Momentum.Common;
using UWPCore.Framework.Controls;
using Windows.UI.Xaml.Navigation;

namespace Momentum.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : UniversalPage
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            AppSettings.UserName.Value = UserNameTextBox.Text;
        }
    }
}
