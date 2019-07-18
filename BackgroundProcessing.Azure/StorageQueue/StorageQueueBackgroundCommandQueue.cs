using System;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using Microsoft.WindowsAzure.Storage.Queue;

namespace BackgroundProcessing.Azure.StorageQueue
{
    /// <summary>
    /// <see cref="IBackgroundCommandQueue"/> for <see cref="CloudQueue"/>.
    /// </summary>
    public class StorageQueueBackgroundCommandQueue : IBackgroundCommandQueue
    {
        private readonly CloudQueue _queue;
        private readonly IBackgroundCommandSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageQueueBackgroundCommandQueue"/> class.
        /// </summary>
        /// <param name="queue">The <see cref="CloudQueue"/>.</param>
        /// <param name="serializer">The <see cref="IBackgroundCommandSerializer"/>.</param>
        public StorageQueueBackgroundCommandQueue(
            CloudQueue queue,
            IBackgroundCommandSerializer serializer)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        /// <inheritdoc />
        public async Task QueueAsync(IBackgroundCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            try
            {
                await _queue.AddMessageAsync(new CloudQueueMessage(await _serializer.SerializeAsync(command)));
            }
            catch (Exception ex)
            {
                throw new BackgroundProcessingException($"Error while enqueueing command {command}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<IBackgroundCommand> DequeueAsync(CancellationToken cancellationToken = default)
        {
            CloudQueueMessage message = null;
            while (message == null && !cancellationToken.IsCancellationRequested)
            {
                message = await _queue.GetMessageAsync();
                await Task.Delay(50);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var command = await _serializer.DeserializeAsync(message.AsString);

            await _queue.DeleteMessageAsync(message);

            return command;
        }
    }
}
