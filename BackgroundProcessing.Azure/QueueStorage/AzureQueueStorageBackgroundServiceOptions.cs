using System;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace BackgroundProcessing.Azure.QueueStorage
{
    /// <summary>
    /// Options for <see cref="AzureQueueStorageBackgroundService"/>.
    /// </summary>
    public class AzureQueueStorageBackgroundServiceOptions
    {
        /// <summary>
        /// Gets or sets the polling interval. Defaults to 50 milliseconds.
        /// </summary>
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMilliseconds(50);

        /// <summary>
        /// Gets or sets the max expected run time for handlers.
        /// This sets the default visibility for messages on the queue when de-queueing.
        /// If the handlers has not finished executing after this time, it is cancelled
        /// (through <see cref="CancellationToken"/> and the message will be visible again (after the <see cref="HandlerCancellationGraceDelay"/>).
        /// Defaults to 15 minutes.
        /// </summary>
        public TimeSpan MaxHandlerRuntime { get; set; } = TimeSpan.FromMinutes(15);

        /// <summary>
        /// Gets or sets the grace delay for handler cancellation in case the <see cref="MaxHandlerRuntime"/> is reached.
        /// Defaults to 30 seconds.
        /// </summary>
        public TimeSpan HandlerCancellationGraceDelay { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets the <see cref="QueueRequestOptions"/>.
        /// </summary>
        public QueueRequestOptions QueueRequestOptions { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OperationContext"/>.
        /// </summary>
        public Func<OperationContext> OperationContextBuilder { get; set; }
    }
}
