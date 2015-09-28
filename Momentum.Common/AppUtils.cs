using System;
using UWPCore.Framework.Common;

namespace Momentum.Common
{
    /// <summary>
    /// A static helper class with common functions regarding the apps functionality.
    /// </summary>
    public static class AppUtils
    {
        private static Localizer localizer = new Localizer("Momentum.Common");

        /// <summary>
        /// Checks whether an update of the data, image, message is required.
        /// </summary>
        /// <param name="latestUpdate">The time of the latest update.</param>
        /// <returns>Returns True when an update is required, else false.</returns>
        public static bool NeedsUpdate(DateTimeOffset latestUpdate)
        {
            return latestUpdate.Date != DateTime.Now.Date
                && DateTime.Now.Hour >= 8;
        }

        /// <summary>
        /// Gets the starting part of the welcome message.
        /// </summary>
        /// <returns>Returns the welcome message start.</returns>
        public static string GetWelcomeMessageStart()
        {
            if (DateTime.Now.Hour < 12)
                return localizer.Get("Welcome.Morning");
            if (DateTime.Now.Hour < 17)
                return localizer.Get("Welcome.Afternoon");
            else
                return localizer.Get("Welcome.Evening");
        }
    }
}
