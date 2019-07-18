using System.Threading.Tasks;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// Dispatches <see cref="IBackgroundCommand"/> for later execution.
    /// </summary>
    public interface IBackgroundDispatcher
    {
        /// <summary>
        /// Dispatches <paramref name="commands"/> for later execution.
        /// </summary>
        /// <param name="commands">The <see cref="IBackgroundCommand"/>s to process.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DispatchAsync(params IBackgroundCommand[] commands);
    }
}
