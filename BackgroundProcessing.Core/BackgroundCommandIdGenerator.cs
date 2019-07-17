using System;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// Helper class to generate sensible Ids for <see cref="IBackgroundCommand"/>.
    /// </summary>
    public static class BackgroundCommandIdGenerator
    {
        /// <summary>
        /// Generates a short unique id based on a <see cref="Guid"/>.
        /// </summary>
        /// <returns>The unique id.</returns>
        public static string Generate() =>
            Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .Substring(0, 22);
    }
}
