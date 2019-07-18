using BackgroundProcessing.Azure.QueueStorage;
using BackgroundProcessing.Core;
using BackgroundProcessing.Core.Serializers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Setup local processing of <see cref="IBackgroundCommand"/> using Azure Queue Storage and a <see cref="BackgroundService"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddHostingServiceAzureQueueStorageBackgroundProcessing(this IServiceCollection services)
        {
            services.TryAddSingleton<IBackgroundCommandSerializer, JsonNetBackgroundCommandSerializer>();
            services.TryAddScoped<IBackgroundProcessor, ServiceProviderBackgroundProcessor>();
            services.TryAddScoped<IBackgroundDispatcher, AzureQueueStorageBackgroundDispatcher>();
            services.AddHostedService<AzureQueueStorageBackgroundService>();

            return services;
        }
    }
}
