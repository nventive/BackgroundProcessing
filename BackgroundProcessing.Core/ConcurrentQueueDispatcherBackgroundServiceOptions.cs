using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// Options for <see cref="ConcurrentQueueDispatcherBackgroundService"/>.
    /// </summary>
    public class ConcurrentQueueDispatcherBackgroundServiceOptions
    {
        /// <summary>
        /// Gets or sets the degree of parallelism in the background processing.
        /// Defaults to the CPU count.
        /// </summary>
        public int DegreeOfParallelism { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Gets or sets a handler that will be notified when an error occurs.
        /// </summary>
        public Func<IBackgroundCommand, Exception, CancellationToken, Task> ErrorHandler { get; set; }
    }
}
