using System;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using Microsoft.Azure.Storage.Queue;

namespace BackgroundProcessing.Azure.Storage.Queue
{
    /// <summary>
    /// This is a helper class for processing Azure Queue Storage commands in an Azure Functions Queue Trigger.
    /// </summary>
    public class AzureFunctionsQueueStorageHandler
    {
        private readonly IBackgroundCommandSerializer _serializer;
        private readonly IBackgroundProcessor _processor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionsQueueStorageHandler"/> class.
        /// </summary>
        /// <param name="serializer">The <see cref="IBackgroundCommandSerializer"/>.</param>
        /// <param name="processor">The <see cref="IBackgroundProcessor"/> to use.</param>
        public AzureFunctionsQueueStorageHandler(
            IBackgroundCommandSerializer serializer,
            IBackgroundProcessor processor)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        }

        /// <summary>
        /// Handles the <paramref name="message"/> from the queue trigger.
        /// </summary>
        /// <param name="message">The <see cref="CloudQueueMessage"/> to handle.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task HandleAsync(CloudQueueMessage message, CancellationToken cancellationToken = default)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await HandleAsync(message.AsString, cancellationToken);
        }

        /// <summary>
        /// Handles the <paramref name="message"/> from the queue trigger.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task HandleAsync(string message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            var command = await _serializer.DeserializeAsync(message);
            await _processor.ProcessAsync(command, cancellationToken);
        }
    }
}
