using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core.HostingService
{
    /// <summary>
    /// <see cref="IBackgroundCommandQueue"/> implementation using <see cref="ConcurrentQueue{T}"/>.
    /// </summary>
    public class ConcurrentBackgroundCommandQueueDispatcher : IBackgroundCommandQueue, IDisposable
    {
        private readonly ConcurrentQueue<IBackgroundCommand> _queue = new ConcurrentQueue<IBackgroundCommand>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
        private bool _disposed = false;

        /// <inheritdoc />
        public async Task QueueAsync(params IBackgroundCommand[] commands)
        {
            if (commands == null)
            {
                throw new ArgumentNullException(nameof(commands));
            }

            foreach (var command in commands)
            {
                if (command != null)
                {
                    _queue.Enqueue(command);
                    _semaphore.Release();
                }
            }
        }

        /// <inheritdoc />
        public async Task<IBackgroundCommand> DequeueAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            _queue.TryDequeue(out var command);
            return command;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">true to dispose managed resources, false otherwise.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _semaphore?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
