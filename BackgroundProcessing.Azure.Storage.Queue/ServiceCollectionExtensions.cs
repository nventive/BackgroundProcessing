using System;
using BackgroundProcessing.Azure.Storage.Queue;
using BackgroundProcessing.Core;
using BackgroundProcessing.Core.Serializers;
using Microsoft.Azure.Storage.Queue;
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
        /// Registers a <see cref="IBackgroundDispatcher"/> for Azure Storage Queue.
        /// You must register a service for <see cref="CloudQueue"/> to use this method.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <param name="configureOptions">To configure the <see cref="CloudQueueBackgroundDispatcherOptions"/> by code.</param>
        /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
        public static BackgroundBuilder AddAzureStorageQueueBackgroundDispatcher(
            this IServiceCollection services,
            Action<CloudQueueBackgroundDispatcherOptions> configureOptions = null)
        {
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            services.TryAddSingleton<IBackgroundCommandSerializer, JsonNetBackgroundCommandSerializer>();
            services.AddScoped<IBackgroundDispatcher, CloudQueueBackgroundDispatcher>();
            return new BackgroundBuilder(services);
        }

        /// <summary>
        /// Setup local processing of <see cref="IBackgroundCommand"/> using Azure Storage Queue and a <see cref="BackgroundService"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <param name="configureOptions">To configure the <see cref="CloudQueueBackgroundServiceOptions"/> by code.</param>
        /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
        public static BackgroundBuilder AddAzureStorageQueueBackgroundProcessing(
            this IServiceCollection services,
            Action<CloudQueueBackgroundServiceOptions> configureOptions = null)
        {
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            services.TryAddSingleton<IBackgroundCommandSerializer, JsonNetBackgroundCommandSerializer>();
            services.TryAddScoped<IBackgroundProcessor, ServiceProviderBackgroundProcessor>();
            services.AddHostedService<CloudQueueBackgroundService>();

            return new BackgroundBuilder(services);
        }

        /// <summary>
        /// Registers processing of commands from Azure Queue Storage in Azure Functions Queue Trigger.
        /// <see cref="AzureFunctionsQueueStorageHandler"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
        public static BackgroundBuilder AddAzureFunctionsQueueStorageProcessing(this IServiceCollection services)
        {
            services.TryAddSingleton<IBackgroundCommandSerializer, JsonNetBackgroundCommandSerializer>();
            services.TryAddScoped<IBackgroundProcessor, ServiceProviderBackgroundProcessor>();
            services.AddScoped<AzureFunctionsQueueStorageHandler>();

            return new BackgroundBuilder(services);
        }
    }
}
