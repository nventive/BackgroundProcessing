using System;
using BackgroundProcessing.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Allows enrichment of the configuration builder to specify dependencies order.
    /// </summary>
    public class BackgroundBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundBuilder"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        public BackgroundBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Gets the <see cref="IServiceCollection"/>.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Decorate the currently registered <see cref="IBackgroundDispatcher"/>.
        /// </summary>
        /// <typeparam name="TDecorator">The decorator type.</typeparam>
        /// <returns>The configured <see cref="BackgroundBuilder"/>.</returns>
        public BackgroundBuilder DecorateDispatcher<TDecorator>()
            where TDecorator : IBackgroundDispatcher
        {
            Services.Decorate<IBackgroundDispatcher, TDecorator>();
            return this;
        }

        /// <summary>
        /// Decorate the currently registered <see cref="IBackgroundDispatcher"/> if it exists, otherwise do nothing.
        /// </summary>
        /// <typeparam name="TDecorator">The decorator type.</typeparam>
        /// <returns>The configured <see cref="BackgroundBuilder"/>.</returns>
        public BackgroundBuilder TryDecorateDispatcher<TDecorator>()
            where TDecorator : IBackgroundDispatcher
        {
            Services.TryDecorate<IBackgroundDispatcher, TDecorator>();
            return this;
        }

        /// <summary>
        /// Decorate the currently registered <see cref="IBackgroundProcessor"/>.
        /// </summary>
        /// <typeparam name="TDecorator">The decorator type.</typeparam>
        /// <returns>The configured <see cref="BackgroundBuilder"/>.</returns>
        public BackgroundBuilder DecorateProcessor<TDecorator>()
            where TDecorator : IBackgroundProcessor
        {
            Services.Decorate<IBackgroundProcessor, TDecorator>();
            return this;
        }

        /// <summary>
        /// Decorate the currently registered <see cref="IBackgroundProcessor"/> if it exists, otherwise do nothing.
        /// </summary>
        /// <typeparam name="TDecorator">The decorator type.</typeparam>
        /// <returns>The configured <see cref="BackgroundBuilder"/>.</returns>
        public BackgroundBuilder TryDecorateProcessor<TDecorator>()
            where TDecorator : IBackgroundProcessor
        {
            Services.TryDecorate<IBackgroundProcessor, TDecorator>();
            return this;
        }
    }
}
