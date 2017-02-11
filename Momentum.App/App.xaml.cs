using System;
using Momentum.App.Views;
using System.Threading.Tasks;
using UWPCore.Framework.Common;
using UWPCore.Framework.Logging;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel;
using UWPCore.Framework.Devices;
using UWPCore.Framework.IoC;
using Momentum.Common.Modules;
using UWPCore.Framework.Store;

namespace Momentum.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : UniversalApp
    {
        private ILicenseService _licenseService;

        public App() : base(typeof(MainPage), AppBackButtonBehaviour.KeepAlive, false, new DefaultModule(), new ReleaseModule())
        {
            InitializeComponent();

            _licenseService = Injector.Get<ILicenseService>();

            ShowShellBackButton = true;

            // initialize Microsoft Application Insights
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
        }

        public async override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            await base.OnInitializeAsync(args);

            var _statusBarService = Injector.Get<IStatusBarService>();
            await _statusBarService.HideAsync();

#if DEBUG
            await _licenseService.RefeshSimulator();
#endif
        }

        public override Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            var launchArgs = args as ILaunchActivatedEventArgs;
            
            if (launchArgs != null )
            {
                if (args.Kind == ActivationKind.Launch)
                {
                    Logger.WriteLine("Started with TILE and launch args: args->{0}; tileId->{1}", launchArgs.Arguments, launchArgs.TileId);
                }
                if (args.Kind == ActivationKind.ToastNotification)
                {
                    Logger.WriteLine("Started with TOAST and launch args: args->{0}; tileId->{1}", launchArgs.Arguments, launchArgs.TileId);
                }
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
