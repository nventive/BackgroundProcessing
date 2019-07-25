namespace BackgroundProcessing.Core.Events
{
    /// <summary>
    /// Statuses fir <see cref="BackgroundCommandEvent"/>.
    /// </summary>
    public enum BackgroundCommandEventStatus
    {
        /// <summary>
        /// The status is unknow.
        /// </summary>
        Unknown,

        /// <summary>
        /// The command has been dispatched, but processing has not started.
        /// </summary>
        Dispatched,

        /// <summary>
        /// Processing is in progress.
        /// </summary>
        Processing,

        /// <summary>
        /// The processing was successful.
        /// </summary>
        Processed,

        /// <summary>
        /// There has been a processing error.
        /// </summary>
        Error,
    }
}
