using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// Handles a <see cref="IBackgroundCommand"/>.
    /// </summary>
    /// <typeparam name="TCommand">The type of <see cref="IBackgroundCommand"/> to handle.</typeparam>
    public interface IBackgroundCommandHandler<TCommand>
        where TCommand : IBackgroundCommand
    {
        /// <summary>
        /// Handles the <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task Handle(TCommand command, CancellationToken cancellationToken = default);
    }
}
