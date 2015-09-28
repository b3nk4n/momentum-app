using Momentum.Common.Models;
using System.Threading.Tasks;

namespace Momentum.Common.Services
{
    /// <summary>
    /// Service interface to update the live tile.
    /// </summary>
    public interface ITileUpdateService
    {
        /// <summary>
        /// Updates the tile notifications.
        /// </summary>
        /// <param name="latestFocus">The latest focus message.</param>
        Task UpdateLiveTile(TodaysFocusModel latestFocus);
    }
}
