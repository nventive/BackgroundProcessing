using System;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// A command that can be dispatched in the background.
    /// </summary>
    public interface IBackgroundCommand
    {
        /// <summary>
        /// Gets the command id.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the timestamp for the creation of the command (in UTC).
        /// </summary>
        DateTimeOffset Timestamp { get; }
    }
}
