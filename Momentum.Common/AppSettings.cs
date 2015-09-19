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
        public static StoredObjectBase<string> UserName = new RoamingObject<string>("_roamingUserName_", string.Empty);
    }
}
