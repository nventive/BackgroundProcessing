using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core
{
    /// <summary>
    /// <see cref="IBackgroundProcessor"/> implementation that uses a <see cref="IServiceProvider"/>
    /// to resolve <see cref="IBackgroundCommandHandler{TCommand}"/>.
    /// </summary>
    public class ServiceProviderBackgroundProcessor : IBackgroundProcessor
    {
        private readonly IServiceProvider _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderBackgroundProcessor"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceProvider"/> to use to resolve <see cref="IBackgroundCommandHandler{TCommand}"/>.</param>
        public ServiceProviderBackgroundProcessor(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <inheritdoc />
        public async Task ProcessAsync(IBackgroundCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var handlerType = typeof(IBackgroundCommandHandler<>).MakeGenericType(command.GetType());

            var handler = _services.GetService(handlerType);
            if (handler == null)
            {
                throw new BackgroundProcessingException($"Unable to find a suitable handler for command {command} ({command.GetType()}).");
            }

            try
            {
                var handleMethod = handlerType.GetMethod("HandleAsync", new[] { command.GetType(), typeof(CancellationToken) });
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
    }
}
