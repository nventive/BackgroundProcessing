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
        /// Dispatches a <paramref name="command"/> for later execution.
        /// </summary>
        /// <param name="command">The <see cref="IBackgroundCommand"/> to process.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task Dispatch(IBackgroundCommand command, CancellationToken cancellationToken = default);
    }
}
