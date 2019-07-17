using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// <see cref="IBackgroundProcessor"/> implementation that uses a <see cref="IServiceProvider"/>
    /// to resolve <see cref="IBackgroundCommandHandler{TCommand}"/>.
    /// Incidentally, it also provides an <see cref="IBackgroundDispatcher"/> implementation
    /// that dispatches to itself. This is useful for local, unit-testing scenarios.
    /// </summary>
    public class ServiceProviderBackgroundProcessor : IBackgroundProcessor, IBackgroundDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderBackgroundProcessor"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use to resolve <see cref="IBackgroundCommandHandler{TCommand}"/>.</param>
        public ServiceProviderBackgroundProcessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public async Task Process(IBackgroundCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var handlerType = typeof(IBackgroundCommandHandler<>).MakeGenericType(command.GetType());

            var handler = _serviceProvider.GetService(handlerType);
            if (handler == null)
            {
                throw new BackgroundProcessingException($"Unable to find a suitable handler for command {command} ({command.GetType()}).");
            }

            try
            {
                var handleMethod = handlerType.GetMethod("Handle", new[] { command.GetType(), typeof(CancellationToken) });
                if (handleMethod == null)
                {
                    throw new BackgroundProcessingException($"Unable to find proper handle method in {handlerType}.");
                }

                handleMethod.Invoke(handler, new object[] { command, cancellationToken });
            }
            catch (BackgroundProcessingException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BackgroundProcessingException($"Error while executing command {command}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public Task Dispatch(IBackgroundCommand command, CancellationToken cancellationToken) => Process(command, cancellationToken);
    }
}
