using System;
using BackgroundProcessing.Core;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;

namespace BackgroundProcessing.Azure.Storage.Queue
{
    /// <summary>
    /// Options for <see cref="CloudQueueBackgroundDispatcher"/>.
    /// </summary>
    public class CloudQueueBackgroundDispatcherOptions
    {
        /// <summary>
        /// Gets or sets the message TTL.
        /// </summary>
        public TimeSpan? TimeToLive { get; set; }

        /// <summary>
        /// Gets or sets the message Initial Visibility Delay.
        /// </summary>
        public TimeSpan? InitialVisibilityDelay { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="QueueRequestOptions"/>.
        /// </summary>
        public QueueRequestOptions QueueRequestOptions { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OperationContext"/>.
        /// </summary>
        public Func<IBackgroundCommand, OperationContext> OperationContextBuilder { get; set; }
    }
}
