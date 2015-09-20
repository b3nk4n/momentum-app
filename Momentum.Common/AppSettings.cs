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
        /// The user focus of today as JSON.
        /// </summary>
        public static RoamingObject<string> TodaysFocusJson = new RoamingObject<string>("HighPriority", string.Empty);
    }
}
