using System;
using UWPCore.Framework.Storage;

namespace Momentum.Common
{
    /// <summary>
    /// The apps settings.
    /// </summary>
    public static class AppSettings
    {
        /// <summary>
        /// The user name of the user.
        /// </summary>
        public static RoamingObject<string> UserName = new RoamingObject<string>("_roamingUserName_", string.Empty);

        /// <summary>
        /// The users wake up time during the week.
        /// </summary>
        public static RoamingObject<TimeSpan> WakeUpTimeWeekDay = new RoamingObject<TimeSpan>("_roamingTimeWeekDay_", new TimeSpan(7, 0, 0));

        /// <summary>
        /// The users wake up time on the weekend.
        /// </summary>
        public static RoamingObject<TimeSpan> WakeUpTimeWeekend = new RoamingObject<TimeSpan>("_roamingTimeWeekend_", new TimeSpan(9, 0, 0));

        /// <summary>
        /// The user focus of today as JSON.
        /// </summary>
        public static RoamingObject<string> TodaysFocusJson = new RoamingObject<string>("HighPriority", string.Empty);
    }
}
