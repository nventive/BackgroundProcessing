using System;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundProcessing.Azure.QueueStorage
{
    /// <summary>
    /// This is a helper class for processing Azure Queue Storage commands in an Azure Functions Queue Trigger.
    /// </summary>
    public class AzureFunctionsQueueStorageHandler
    {
        private readonly IBackgroundCommandSerializer _serializer;
        private readonly IServiceProvider _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionsQueueStorageHandler"/> class.
        /// </summary>
        /// <param name="serializer">The <see cref="IBackgroundCommandSerializer"/>.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to manage scopes.</param>
        public AzureFunctionsQueueStorageHandler(
            IBackgroundCommandSerializer serializer,
            IServiceProvider services)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _services = services ?? throw new ArgumentNullException(nameof(services));
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

            var processor = _services.GetRequiredService<IBackgroundProcessor>();
            await processor.ProcessAsync(command, cancellationToken);
        }
    }
}
