using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// Simple <see cref="IBackgroundCommand"/> queue mechanism.
    /// </summary>
    public interface IBackgroundCommandQueue
    {
        /// <summary>
        /// Queue The <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The <see cref="IBackgroundCommand"/> to queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task QueueAsync(IBackgroundCommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dequeue <see cref="IBackgroundCommand"/>. This is a BLOCKING operation.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The dequeued <see cref="IBackgroundCommand"/>.</returns>
        Task<IBackgroundCommand> DequeueAsync(CancellationToken cancellationToken = default);
    }
}
