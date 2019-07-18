using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// Serializes and Deserializes <see cref="IBackgroundCommand"/> as a <see cref="string"/>.
    /// </summary>
    public interface IBackgroundCommandSerializer
    {
        /// <summary>
        /// Serializes a <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The <see cref="IBackgroundCommand"/> to serialize.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The serialized <paramref name="command"/>.</returns>
        Task<string> SerializeAsync(IBackgroundCommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserialize <paramref name="value"/> into a <see cref="IBackgroundCommand"/>.
        /// </summary>
        /// <param name="value">The input text.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The deserialized <see cref="IBackgroundCommand"/>.</returns>
        Task<IBackgroundCommand> DeserializeAsync(string value, CancellationToken cancellationToken = default);
    }
}
