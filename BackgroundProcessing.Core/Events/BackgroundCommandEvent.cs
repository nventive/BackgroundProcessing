using System;

namespace BackgroundProcessing.Core.Events
{
    /// <summary>
    /// Lifecycle events for the execution of <see cref="IBackgroundCommand"/>.
    /// </summary>
    public class BackgroundCommandEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundCommandEvent"/> class.
        /// </summary>
        /// <param name="command">The <see cref="IBackgroundCommand"/>.</param>
        /// <param name="status">The <see cref="BackgroundCommandEventStatus"/>.</param>
        /// <param name="timestamp">The timestamp of the event.</param>
        /// <param name="exception">The <see cref="Exception"/>, if any.</param>
        public BackgroundCommandEvent(
            IBackgroundCommand command,
            BackgroundCommandEventStatus status,
            DateTimeOffset timestamp,
            Exception exception = null)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Status = status;
            Timestamp = timestamp;
            Exception = exception;
        }

        /// <summary>
        /// Gets the <see cref="IBackgroundCommand"/>.
        /// </summary>
        public IBackgroundCommand Command { get; }

        /// <summary>
        /// Gets the <see cref="BackgroundCommandEventStatus"/>.
        /// </summary>
        public BackgroundCommandEventStatus Status { get; }

        /// <summary>
        /// Gets the timestamp of the event.
        /// </summary>
        public DateTimeOffset Timestamp { get; }

        /// <summary>
        /// Gets the corresponding <see cref="Exception"/>, if any.
        /// </summary>
        public Exception Exception { get; }
    }
}
