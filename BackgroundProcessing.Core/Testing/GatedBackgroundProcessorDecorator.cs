using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core.Testing
{
    /// <summary>
    /// <see cref="IBackgroundProcessor"/> decorator that can wait for a number of invocation.
    /// This is very useful in testing scenarios. DO NOT USE IT IN PRODUCTION.
    /// </summary>
    public sealed class GatedBackgroundProcessorDecorator : IBackgroundProcessor
    {
        private readonly IBackgroundProcessor _wrappedProcessor;
        private readonly GatedBackgroundProcessorAwaiter _awaiter;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatedBackgroundProcessorDecorator"/> class.
        /// </summary>
        /// <param name="wrappedProcessor">The wrapped <see cref="IBackgroundProcessor"/>.</param>
        /// <param name="awaiter">The <see cref="GatedBackgroundProcessorAwaiter"/>.</param>
        public GatedBackgroundProcessorDecorator(
            IBackgroundProcessor wrappedProcessor,
            GatedBackgroundProcessorAwaiter awaiter)
        {
            _wrappedProcessor = wrappedProcessor ?? throw new ArgumentNullException(nameof(wrappedProcessor));
            _awaiter = awaiter;
        }

        /// <inheritdoc />
        public async Task ProcessAsync(IBackgroundCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                await _wrappedProcessor.ProcessAsync(command, cancellationToken);
            }
            finally
            {
                _awaiter.Signal();
            }
        }
    }
}
