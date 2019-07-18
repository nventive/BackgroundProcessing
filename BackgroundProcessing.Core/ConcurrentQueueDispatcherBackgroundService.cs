using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// <see cref="IBackgroundDispatcher"/> and <see cref="BackgroundService"/> implementation that uses
    /// an in-memory <see cref="ConcurrentQueue{T}"/> and <see cref="SemaphoreSlim"/>.
    /// </summary>
    public class ConcurrentQueueDispatcherBackgroundService : BackgroundService, IBackgroundDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private readonly ConcurrentQueue<IBackgroundCommand> _queue = new ConcurrentQueue<IBackgroundCommand>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentQueueDispatcherBackgroundService"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceProvider"/> used to manage scopes.</param>
        /// <param name="logger">The <see cref="ILogger"/> used to signal exceptions.</param>
        public ConcurrentQueueDispatcherBackgroundService(
            IServiceProvider services,
            ILogger<ConcurrentQueueDispatcherBackgroundService> logger)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task DispatchAsync(IBackgroundCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            _queue.Enqueue(command);
            _semaphore.Release();
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "There is no good way to manage errors at the moment.")]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _semaphore.WaitAsync(stoppingToken);
                _queue.TryDequeue(out var command);

                if (command != null)
                {
                    try
                    {
                        using (var scope = _services.CreateScope())
                        {
                            var processor = scope.ServiceProvider.GetRequiredService<IBackgroundProcessor>();
                            await processor.ProcessAsync(command, stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An error occured while processing {command}: {ex.Message}", ex);
                    }
                }
            }
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
