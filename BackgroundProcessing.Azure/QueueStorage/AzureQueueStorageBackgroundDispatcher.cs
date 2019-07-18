using System;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Queue;

namespace BackgroundProcessing.Azure.QueueStorage
{
    /// <summary>
    /// <see cref="IBackgroundDispatcher"/> for <see cref="CloudQueue"/>.
    /// </summary>
    public class AzureQueueStorageBackgroundDispatcher : IBackgroundDispatcher
    {
        private readonly IOptions<AzureQueueStorageBackgroundDispatcherOptions> _options;
        private readonly CloudQueue _queue;
        private readonly IBackgroundCommandSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureQueueStorageBackgroundDispatcher"/> class.
        /// </summary>
        /// <param name="options">The <see cref="AzureQueueStorageBackgroundDispatcherOptions"/>.</param>
        /// <param name="queue">The <see cref="CloudQueue"/>.</param>
        /// <param name="serializer">The <see cref="IBackgroundCommandSerializer"/>.</param>
        public AzureQueueStorageBackgroundDispatcher(
            IOptions<AzureQueueStorageBackgroundDispatcherOptions> options,
            CloudQueue queue,
            IBackgroundCommandSerializer serializer)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        /// <inheritdoc />
        public async Task DispatchAsync(IBackgroundCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            try
            {
                var options = _options.Value;
                await _queue.AddMessageAsync(
                    new CloudQueueMessage(await _serializer.SerializeAsync(command)),
                    timeToLive: options.TimeToLive,
                    initialVisibilityDelay: options.InitialVisibilityDelay,
                    options: options.QueueRequestOptions,
                    operationContext: options.OperationContextBuilder != null ? options.OperationContextBuilder(command) : null,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                throw new BackgroundProcessingException($"Error while enqueueing command {command}: {ex.Message}", ex);
            }
        }
    }
}
