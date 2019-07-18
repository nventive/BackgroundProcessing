using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundProcessing.Core.HostingService
{
    /// <summary>
    /// <see cref="BackgroundService"/> that dequeues <see cref="IBackgroundCommand"/>s from
    /// <see cref="IBackgroundCommandQueue"/> and process them through <see cref="IBackgroundProcessor"/>.
    /// Execution is handle through a scope.
    /// </summary>
    public class BackgroundCommandQueueService : BackgroundService
    {
        private readonly IBackgroundCommandQueue _queue;
        private readonly IServiceProvider _services;
        private readonly ILogger<BackgroundCommandQueueService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundCommandQueueService"/> class.
        /// </summary>
        /// <param name="queue">The <see cref="IBackgroundCommandQueue"/>.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to manage scopes.</param>
        /// <param name="logger">The <see cref="ILogger"/> used to signal exceptions.</param>
        public BackgroundCommandQueueService(
            IBackgroundCommandQueue queue,
            IServiceProvider services,
            ILogger<BackgroundCommandQueueService> logger)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "There is no good way to manage errors at the moment.")]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var command = await _queue.DequeueAsync(stoppingToken);

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
}
