using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core.Events
{
    /// <summary>
    /// Allows tracking and querying of <see cref="BackgroundCommandEvent"/>.
    /// </summary>
    public interface IBackgroundCommandEventRepository
    {
        /// <summary>
        /// Adds a <see cref="BackgroundCommandEvent"/>.
        /// </summary>
        /// <param name="commandEvent">The <see cref="BackgroundCommandEvent"/> to add.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task Add(BackgroundCommandEvent commandEvent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the latest event for the <paramref name="commandId"/>.
        /// </summary>
        /// <param name="commandId">The <see cref="IBackgroundCommand.Id"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The most recent <see cref="BackgroundCommandEvent"/>, if any.</returns>
        Task<BackgroundCommandEvent> GetLatestEventForCommandId(string commandId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all the events for the <paramref name="commandId"/>.
        /// </summary>
        /// <param name="commandId">The <see cref="IBackgroundCommand.Id"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The events in descending chronological order.</returns>
        Task<IEnumerable<BackgroundCommandEvent>> GetAllEventsForCommandId(string commandId, CancellationToken cancellationToken = default);
    }
}
