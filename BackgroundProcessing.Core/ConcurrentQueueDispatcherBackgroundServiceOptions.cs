using System;
using System.Threading;

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
    }
}
