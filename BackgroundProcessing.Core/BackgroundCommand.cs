using System;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// Base class for <see cref="IBackgroundCommand"/> implementations.
    /// </summary>
    public abstract class BackgroundCommand : IBackgroundCommand
    {
        /// <inheritdoc />
        public string Id { get; set; } = BackgroundCommandIdGenerator.Generate();

        /// <inheritdoc />
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
