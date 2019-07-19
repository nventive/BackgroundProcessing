using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// <see cref="IBackgroundDispatcher"/> and <see cref="BackgroundService"/> implementation that uses
    /// an in-memory <see cref="ConcurrentQueue{T}"/> and <see cref="SemaphoreSlim"/>.
    /// </summary>
    public class ConcurrentQueueDispatcherBackgroundService : BackgroundService, IBackgroundDispatcher
    {
        private readonly IOptions<ConcurrentQueueDispatcherBackgroundServiceOptions> _options;
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private readonly ConcurrentQueue<IBackgroundCommand> _queue = new ConcurrentQueue<IBackgroundCommand>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentQueueDispatcherBackgroundService"/> class.
        /// </summary>
        /// <param name="options">The <see cref="ConcurrentQueueDispatcherBackgroundServiceOptions"/>.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to manage scopes.</param>
        /// <param name="logger">The <see cref="ILogger"/> used to signal exceptions.</param>
        public ConcurrentQueueDispatcherBackgroundService(
            IOptions<ConcurrentQueueDispatcherBackgroundServiceOptions> options,
            IServiceProvider services,
            ILogger<ConcurrentQueueDispatcherBackgroundService> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
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
            var options = _options.Value;

            var processCommandsActionBlock = new ActionBlock<IBackgroundCommand>(
                async command =>
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
                        await options.ErrorHandler?.Invoke(command, ex, stoppingToken);
                    }
                },
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = stoppingToken,
                    BoundedCapacity = options.DegreeOfParallelism,
                    MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
                });

            while (!stoppingToken.IsCancellationRequested)
            {
                await _semaphore.WaitAsync(stoppingToken);
                _queue.TryDequeue(out var command);

                if (command != null)
                {
                    await processCommandsActionBlock.SendAsync(command, stoppingToken);
                }
            }

            processCommandsActionBlock.Complete();
            await processCommandsActionBlock.Completion;
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
