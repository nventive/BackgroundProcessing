using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// Dispatches <see cref="IBackgroundCommand"/> for later execution.
    /// </summary>
    public interface IBackgroundDispatcher
    {
        /// <summary>
        /// Dispatches <paramref name="command"/> for later execution.
        /// </summary>
        /// <param name="command">The <see cref="IBackgroundCommand"/>s to dispatch.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DispatchAsync(IBackgroundCommand command, CancellationToken cancellationToken = default);
    }
}
