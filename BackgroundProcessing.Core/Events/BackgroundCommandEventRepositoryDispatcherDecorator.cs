using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core.Events
{
    /// <summary>
    /// <see cref="IBackgroundDispatcher"/> decorator that sends events to <see cref="IBackgroundCommandEventRepository"/>.
    /// </summary>
    public class BackgroundCommandEventRepositoryDispatcherDecorator : IBackgroundDispatcher
    {
        private readonly IBackgroundDispatcher _wrappedDispatcher;
        private readonly IBackgroundCommandEventRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundCommandEventRepositoryDispatcherDecorator"/> class.
        /// </summary>
        /// <param name="wrappedDispatcher">The wrapped <see cref="IBackgroundDispatcher"/>.</param>
        /// <param name="repository">The <see cref="IBackgroundCommandEventRepository"/>.</param>
        public BackgroundCommandEventRepositoryDispatcherDecorator(
            IBackgroundDispatcher wrappedDispatcher,
            IBackgroundCommandEventRepository repository)
        {
            _wrappedDispatcher = wrappedDispatcher ?? throw new ArgumentNullException(nameof(wrappedDispatcher));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc />
        public async Task DispatchAsync(IBackgroundCommand command, CancellationToken cancellationToken = default)
        {
            // We intentionally send the dispatching event first, otherwise we are running into concurrency issue with the processing.
            await _repository.Add(new BackgroundCommandEvent(command, BackgroundCommandEventStatus.Dispatching, DateTimeOffset.UtcNow));
            try
            {
                await _wrappedDispatcher.DispatchAsync(command, cancellationToken);
            }
            catch (Exception ex)
            {
                await _repository.Add(new BackgroundCommandEvent(command, BackgroundCommandEventStatus.Error, DateTimeOffset.UtcNow, ex));
                throw;
            }
        }
    }
}
