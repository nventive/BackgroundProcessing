using System;
using BackgroundProcessing.Core;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="BackgroundBuilder"/>.
    /// </summary>
    public static class BackgroundBuilderExtensions
    {
        /// <summary>
        /// Registers <see cref="CloudQueue"/> as a singleton using the <paramref name="connectionStringName"/> and <paramref name="queueName"/>.
        /// Will create the queue if it does not exists.
        /// </summary>
        /// <param name="builder">The <see cref="BackgroundBuilder"/>.</param>
        /// <param name="connectionStringName">The name of the connection string to the storage account.</param>
        /// <param name="queueName">The queue name.</param>
        /// <returns>The configured <see cref="BackgroundBuilder"/>.</returns>
        public static BackgroundBuilder ConfigureCloudQueueUsingConnectionStringName(
            this BackgroundBuilder builder,
            string connectionStringName,
            string queueName)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString(connectionStringName);
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new BackgroundProcessingException(
                        $"Unable to find connection string with the name {connectionStringName} while building the queue {queueName}.");
                }

                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var queueClient = storageAccount.CreateCloudQueueClient();
                var queue = queueClient.GetQueueReference(queueName);
                queue.CreateIfNotExistsAsync().Wait();

                return queue;
            });

            return builder;
        }

        /// <summary>
        /// Registers <see cref="CloudQueue"/> as a singleton using the <paramref name="connectionString"/> and <paramref name="queueName"/>.
        /// Will create the queue if it does not exists.
        /// </summary>
        /// <param name="builder">The <see cref="BackgroundBuilder"/>.</param>
        /// <param name="connectionString">The connection string to the storage account.</param>
        /// <param name="queueName">The queue name.</param>
        /// <returns>The configured <see cref="BackgroundBuilder"/>.</returns>
        public static BackgroundBuilder ConfigureCloudQueueUsingConnectionString(
            this BackgroundBuilder builder,
            string connectionString,
            string queueName)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton(sp =>
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var queueClient = storageAccount.CreateCloudQueueClient();
                var queue = queueClient.GetQueueReference(queueName);
                queue.CreateIfNotExistsAsync().Wait();

                return queue;
            });

            return builder;
        }
    }
}
