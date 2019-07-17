using BackgroundProcessing.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services necessary to process background tasks locally. <see cref="IBackgroundProcessor"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddLocalBackgroundProcessor(this IServiceCollection services)
            => services.AddScoped<IBackgroundProcessor, ServiceProviderBackgroundProcessor>();
    }
}
