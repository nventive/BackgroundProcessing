using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core.Events;

namespace BackgroundProcessing.Caching
{
    /// <summary>
    /// Serializes and Deserializes <see cref="BackgroundCommandEvent"/> as a <see cref="string"/>.
    /// </summary>
    public interface IBackgroundCommandEventsSerializer
    {
        /// <summary>
        /// Serializes a <see cref="BackgroundCommandEvent"/>.
        /// </summary>
        /// <param name="commandEvent">The <see cref="BackgroundCommandEvent"/> to serialize.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The serialized <paramref name="commandEvent"/>.</returns>
        Task<string> SerializeAsync(IEnumerable<BackgroundCommandEvent> commandEvent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserialize <paramref name="value"/> into a <see cref="BackgroundCommandEvent"/>.
        /// </summary>
        /// <param name="value">The input text.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The deserialized <see cref="BackgroundCommandEvent"/>.</returns>
        Task<IList<BackgroundCommandEvent>> DeserializeAsync(string value, CancellationToken cancellationToken = default);
    }
}
