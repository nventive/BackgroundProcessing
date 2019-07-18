using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// <see cref="IBackgroundDispatcher"/> implementation that pushes <see cref="IBackgroundCommand"/> to <see cref="IBackgroundCommandQueue"/>.
    /// </summary>
    public class BackgroundCommandQueueDispatcher : IBackgroundDispatcher
    {
        private readonly IBackgroundCommandQueue _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundCommandQueueDispatcher"/> class.
        /// </summary>
        /// <param name="queue">The <see cref="IBackgroundCommandQueue"/> to use.</param>
        public BackgroundCommandQueueDispatcher(IBackgroundCommandQueue queue)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        /// <inheritdoc />
        public Task DispatchAsync(IBackgroundCommand command, CancellationToken cancellationToken = default) => _queue.QueueAsync(command, cancellationToken);
    }
}
