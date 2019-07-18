using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Queue;

namespace BackgroundProcessing.Azure.QueueStorage
{
    /// <summary>
    /// <see cref="BackgroundService"/> implementation that uses <see cref="CloudQueue"/>.
    /// </summary>
    public class AzureQueueStorageBackgroundService : BackgroundService
    {
        private readonly IOptions<AzureQueueStorageBackgroundServiceOptions> _options;
        private readonly CloudQueue _queue;
        private readonly IBackgroundCommandSerializer _serializer;
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureQueueStorageBackgroundService"/> class.
        /// </summary>
        /// <param name="options">The <see cref="AzureQueueStorageBackgroundServiceOptions"/>.</param>
        /// <param name="queue">The <see cref="CloudQueue"/>.</param>
        /// <param name="serializer">The <see cref="IBackgroundCommandSerializer"/>.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to manage scopes.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public AzureQueueStorageBackgroundService(
            IOptions<AzureQueueStorageBackgroundServiceOptions> options,
            CloudQueue queue,
            IBackgroundCommandSerializer serializer,
            IServiceProvider services,
            ILogger<AzureQueueStorageBackgroundService> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "There is no good way to manage errors at the moment.")]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var options = _options.Value;
                await Task.Delay(options.PollingInterval);
                var message = await _queue.GetMessageAsync(
                    visibilityTimeout: options.MaxHandlerRuntime.Add(options.HandlerCancellationGraceDelay),
                    options: options.QueueRequestOptions,
                    operationContext: options.OperationContextBuilder != null ? options.OperationContextBuilder() : null);

                if (message == null)
                {
                    continue;
                }

                var command = await _serializer.DeserializeAsync(message.AsString);

                try
                {
                    using (var handlerRuntimeCancellationTokenSource = new CancellationTokenSource(options.MaxHandlerRuntime))
                    using (var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, handlerRuntimeCancellationTokenSource.Token))
                    using (var scope = _services.CreateScope())
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<IBackgroundProcessor>();
                        await processor.ProcessAsync(command, combinedCancellationTokenSource.Token);
                        combinedCancellationTokenSource.Token.ThrowIfCancellationRequested();
                        await _queue.DeleteMessageAsync(message);
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
