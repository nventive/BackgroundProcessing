using System;
using BackgroundProcessing.Azure.QueueStorage;
using BackgroundProcessing.Core;
using BackgroundProcessing.Core.Serializers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a <see cref="IBackgroundDispatcher"/> for Azure Queue Storage.
        /// You must register a service for <see cref="CloudQueue"/> to use this method.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <param name="configureOptions">To configure the <see cref="AzureQueueStorageBackgroundDispatcherOptions"/> by code.</param>
        /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddAzureQueueStorageBackgroundDispatcher(
            this IServiceCollection services,
            Action<AzureQueueStorageBackgroundDispatcherOptions> configureOptions = null)
        {
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            services.TryAddSingleton<IBackgroundCommandSerializer, JsonNetBackgroundCommandSerializer>();
            services.AddScoped<IBackgroundDispatcher, AzureQueueStorageBackgroundDispatcher>();
            return services;
        }

        /// <summary>
        /// Setup local processing of <see cref="IBackgroundCommand"/> using Azure Queue Storage and a <see cref="BackgroundService"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <param name="configureOptions">To configure the <see cref="AzureQueueStorageBackgroundServiceOptions"/> by code.</param>
        /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddAzureQueueStorageBackgroundProcessing(
            this IServiceCollection services,
            Action<AzureQueueStorageBackgroundServiceOptions> configureOptions = null)
        {
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            services.TryAddSingleton<IBackgroundCommandSerializer, JsonNetBackgroundCommandSerializer>();
            services.TryAddScoped<IBackgroundProcessor, ServiceProviderBackgroundProcessor>();
            services.AddHostedService<AzureQueueStorageBackgroundService>();

            return services;
        }

        /// <summary>
        /// Registers processing of commands from Azure Queue Storage in Azure Functions Queue Trigger.
        /// <see cref="AzureFunctionsQueueStorageHandler"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddAzureFunctionsQueueStorageProcessing(this IServiceCollection services)
        {
            services.TryAddSingleton<IBackgroundCommandSerializer, JsonNetBackgroundCommandSerializer>();
            services.TryAddScoped<IBackgroundProcessor, ServiceProviderBackgroundProcessor>();
            services.AddScoped<AzureFunctionsQueueStorageHandler>();

            return services;
        }
    }
}
