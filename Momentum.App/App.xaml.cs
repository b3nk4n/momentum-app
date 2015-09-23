using Momentum.App.Views;
using System.Threading.Tasks;
using UWPCore.Framework.Common;
using UWPCore.Framework.Logging;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel;
using UWPCore.Framework.Devices;

namespace Momentum.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : UniversalApp
    {
        public App() : base(typeof(MainPage), AppBackButtonBehaviour.Terminate, "Momentum.App")
        {
            InitializeComponent();

            ShowShellBackButton = true;

            // initialize Microsoft Application Insights
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
        }

        public async override Task OnInitializeAsync()
        {
            await base.OnInitializeAsync();

            var _statusBarService = new StatusBarService();
            await _statusBarService.HideAsync();
        }

        public override Task OnStartAsync(StartKind startKind, IActivatedEventArgs args, ILaunchArgs launchArgs)
        {
            // check lauch arguments
            if (launchArgs.IsValid)
            {
                Logger.WriteLine("Started with launch args: args->{0}; tileId->{1}", launchArgs.Arguments, launchArgs.TileId);
                // TODO: launched by toast notification?
            }

            // start the user experience
            NavigationService.Navigate(DefaultPage);
            return Task.FromResult<object>(null);
        }

        public override Task OnSuspendingAsync(SuspendingEventArgs e)
        {
            return base.OnSuspendingAsync(e);
        }
    }
}
