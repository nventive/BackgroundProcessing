using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace BackgroundProcessing.Azure.QueueStorage
{
    /// <summary>
    /// Options for <see cref="AzureQueueStorageBackgroundService"/>.
    /// </summary>
    public class AzureQueueStorageBackgroundServiceOptions
    {
        private static readonly Func<TimeSpan, TimeSpan> DefaultPollingFrequency = new Func<TimeSpan, TimeSpan>(x =>
        {
            if (Debugger.IsAttached)
            {
                return TimeSpan.FromSeconds(1);
            }

            var next = new TimeSpan((long)(x.Ticks * 1.3));
            return next > TimeSpan.FromMinutes(1) ? TimeSpan.FromMinutes(1) : next;
        });

        /// <summary>
        /// Gets or sets the degree of parallelism in the background processing.
        /// Defaults to the CPU count.
        /// </summary>
        public int DegreeOfParallelism { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Gets or sets the batch size when retrieving messages.
        /// Defaults to the CPU count.
        /// </summary>
        public int MessagesBatchSize { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Gets or sets the initial queue polling frequency when there are messages.
        /// Defaults to 1 second.
        /// </summary>
        public TimeSpan PollingFrequency { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Gets or sets the algorithm to determine the next polling frequency.
        /// Default strategy: multiply previous by 1.3, with a maximum of 1 minute.
        /// If a debugger is attached, it polls every 1 second no matter what.
        /// The next polling frequency is reset to <see cref="PollingFrequency"/> when a new message arrives.
        /// </summary>
        public Func<TimeSpan, TimeSpan> NextPollingFrequency { get; set; } = DefaultPollingFrequency;

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

        /// <summary>
        /// Gets or sets a handler that will be notified when an error occurs.
        /// </summary>
        public Func<IBackgroundCommand, Exception, CancellationToken, Task> ErrorHandler { get; set; }
    }
}
