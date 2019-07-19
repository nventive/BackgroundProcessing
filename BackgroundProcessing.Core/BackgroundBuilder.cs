using System;

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
    }
}
