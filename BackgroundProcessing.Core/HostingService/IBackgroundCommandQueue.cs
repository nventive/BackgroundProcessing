using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core.HostingService
{
    /// <summary>
    /// Simple <see cref="IBackgroundCommand"/> queue mechanism.
    /// </summary>
    public interface IBackgroundCommandQueue
    {
        /// <summary>
        /// Queue <paramref name="commands"/>.
        /// </summary>
        /// <param name="commands">The <see cref="IBackgroundCommand"/>s to queue.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task QueueAsync(params IBackgroundCommand[] commands);

        /// <summary>
        /// Dequeue <see cref="IBackgroundCommand"/>. This is a BLOCKING operation.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The dequeued <see cref="IBackgroundCommand"/>.</returns>
        Task<IBackgroundCommand> DequeueAsync(CancellationToken cancellationToken = default);
    }
}
