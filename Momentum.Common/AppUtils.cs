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
            // only update when we have a new day
            if (latestUpdate.Date == DateTimeOffset.Now.Date)
                return false;

            var currentTimeSpan = DateTimeOffset.Now - DateTimeOffset.Now.Date;

            switch (DateTimeOffset.Now.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday:
                    return currentTimeSpan > AppSettings.WakeUpTimeWeekend.Value;

                default:
                    return currentTimeSpan > AppSettings.WakeUpTimeWeekDay.Value;
            }
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
