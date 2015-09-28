using Momentum.Common;
using System.Threading.Tasks;
using UWPCore.Framework.Controls;
using Windows.UI.Xaml.Navigation;

namespace Momentum.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : UniversalPage
    {
        public const string PARAM_CHANGE_NAME = "changeName";

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            UserNameTextBox.Text = AppSettings.UserName.Value;

            if (e.Parameter.ToString() == PARAM_CHANGE_NAME)
            {
                // wait shortly until the bindings have been taken
                await Task.Delay(500);

                UserNameTextBox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                UserNameTextBox.SelectAll();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            AppSettings.UserName.Value = UserNameTextBox.Text;
        }
    }
}
