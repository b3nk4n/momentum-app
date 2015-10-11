using Momentum.Common.Services;
using Ninject.Modules;
using Windows.Globalization;

namespace Momentum.Common.Modules
{
    /// <summary>
    /// The IoC module for the release app.
    /// </summary>
    public class ReleaseModule : NinjectModule
    {
        public override void Load()
        {
            var language = ApplicationLanguages.Languages[0];
            Bind<IImageService>().To<BingImageService>().InSingletonScope().WithConstructorArgument<string>(language);
            Bind<IQuoteService>().To<QuoteService>().InSingletonScope().WithConstructorArgument<string>(language);
            Bind<ITileUpdateService>().To<TileUpdateService>().InSingletonScope().WithConstructorArgument<string>(language);
        }
    }
}
