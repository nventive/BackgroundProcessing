using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// Allows submission of <see cref="IBackgroundCommand"/>.
    /// </summary>
    public interface IBackgroundProcessor
    {
        /// <summary>
        /// Processes a <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The <see cref="IBackgroundCommand"/> to process.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task Process(IBackgroundCommand command, CancellationToken cancellationToken = default);
    }
}
