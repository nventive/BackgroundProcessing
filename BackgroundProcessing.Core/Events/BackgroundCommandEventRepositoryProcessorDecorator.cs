using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core.Events
{
    /// <summary>
    /// <see cref="IBackgroundProcessor"/> decorator that sends events to <see cref="IBackgroundCommandEventRepository"/>.
    /// </summary>
    public class BackgroundCommandEventRepositoryProcessorDecorator : IBackgroundProcessor
    {
        private readonly IBackgroundProcessor _wrappedProcessor;
        private readonly IBackgroundCommandEventRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundCommandEventRepositoryProcessorDecorator"/> class.
        /// </summary>
        /// <param name="wrappedProcessor">The wrapped <see cref="IBackgroundProcessor"/>.</param>
        /// <param name="repository">The <see cref="IBackgroundCommandEventRepository"/>.</param>
        public BackgroundCommandEventRepositoryProcessorDecorator(
            IBackgroundProcessor wrappedProcessor,
            IBackgroundCommandEventRepository repository)
        {
            _wrappedProcessor = wrappedProcessor ?? throw new ArgumentNullException(nameof(wrappedProcessor));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc />
        public async Task ProcessAsync(IBackgroundCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                await _repository.Add(new BackgroundCommandEvent(command, BackgroundCommandEventStatus.Processing, DateTimeOffset.UtcNow), cancellationToken);
                await _wrappedProcessor.ProcessAsync(command, cancellationToken);
                await _repository.Add(new BackgroundCommandEvent(command, BackgroundCommandEventStatus.Processed, DateTimeOffset.UtcNow));
            }
            catch (Exception ex)
            {
                await _repository.Add(new BackgroundCommandEvent(command, BackgroundCommandEventStatus.Error, DateTimeOffset.UtcNow, ex));
                throw;
            }
        }
    }
}
