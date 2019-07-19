using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BackgroundProcessing.Core;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackgroundProcessing.Azure.Storage.Queue
{
    /// <summary>
    /// <see cref="BackgroundService"/> implementation that uses <see cref="CloudQueue"/>.
    /// </summary>
    public class CloudQueueBackgroundService : BackgroundService
    {
        private readonly IOptions<CloudQueueBackgroundServiceOptions> _options;
        private readonly CloudQueue _queue;
        private readonly IBackgroundCommandSerializer _serializer;
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudQueueBackgroundService"/> class.
        /// </summary>
        /// <param name="options">The <see cref="CloudQueueBackgroundServiceOptions"/>.</param>
        /// <param name="queue">The <see cref="CloudQueue"/>.</param>
        /// <param name="serializer">The <see cref="IBackgroundCommandSerializer"/>.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to manage scopes.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public CloudQueueBackgroundService(
            IOptions<CloudQueueBackgroundServiceOptions> options,
            CloudQueue queue,
            IBackgroundCommandSerializer serializer,
            IServiceProvider services,
            ILogger<CloudQueueBackgroundService> logger)
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
            var options = _options.Value;

            var processMessagesActionBlock = new ActionBlock<CloudQueueMessage>(
                async message =>
                {
                    IBackgroundCommand command = null;
                    try
                    {
                        command = await _serializer.DeserializeAsync(message.AsString);

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
                        await options.ErrorHandler?.Invoke(command, ex, stoppingToken);
                    }
                },
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = stoppingToken,
                    BoundedCapacity = options.DegreeOfParallelism,
                    MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
                });

            var currentPollingFrequency = options.PollingFrequency;
            while (!stoppingToken.IsCancellationRequested)
            {
                var messages = await _queue.GetMessagesAsync(
                    messageCount: options.MessagesBatchSize,
                    visibilityTimeout: options.MaxHandlerRuntime.Add(options.HandlerCancellationGraceDelay),
                    options: options.QueueRequestOptions,
                    operationContext: options.OperationContextBuilder != null ? options.OperationContextBuilder() : null);

                if (!messages.Any())
                {
                    await Task.Delay(currentPollingFrequency);
                    currentPollingFrequency = options.NextPollingFrequency(currentPollingFrequency);
                    continue;
                }

                currentPollingFrequency = options.PollingFrequency;

                foreach (var message in messages)
                {
                    await processMessagesActionBlock.SendAsync(message, stoppingToken);
                }
            }

            processMessagesActionBlock.Complete();
            await processMessagesActionBlock.Completion;
        }
    }
}
