using System;

namespace Momentum.Common
{
    /// <summary>
    /// A static helper class with common functions regarding the apps functionality.
    /// </summary>
    public static class AppHelpers
    {
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
    }
}
