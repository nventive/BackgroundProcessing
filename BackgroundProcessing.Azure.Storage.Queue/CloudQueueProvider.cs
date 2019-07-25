using System;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Helper class to initialize <see cref="CloudQueue"/> instances.
    /// </summary>
    public static class CloudQueueProvider
    {
        /// <summary>
        /// Returns a <see cref="CloudQueue"/> provider initialized from a connection string name and a queue name.
        /// Will create the queue if it does not exists.
        /// </summary>
        /// <param name="connectionStringName">The name of the connection string.</param>
        /// <param name="queueName">The storage queue name.</param>
        /// <returns>The <see cref="CloudQueue"/> provider.</returns>
        public static Func<IServiceProvider, CloudQueue> FromConnectionStringName(
            string connectionStringName,
            string queueName)
        {
            return (sp) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString(connectionStringName);
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var queueClient = storageAccount.CreateCloudQueueClient();
                var cloudQueue = queueClient.GetQueueReference(queueName);
                cloudQueue.CreateIfNotExists();

                return cloudQueue;
            };
        }

        /// <summary>
        /// Returns a <see cref="CloudQueue"/> provider initialized from a connection string and a queue name.
        /// Will create the queue if it does not exists.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="queueName">The storage queue name.</param>
        /// <returns>The <see cref="CloudQueue"/> provider.</returns>
        public static Func<IServiceProvider, CloudQueue> FromConnectionString(
            string connectionString,
            string queueName)
        {
            return (sp) =>
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var queueClient = storageAccount.CreateCloudQueueClient();
                var cloudQueue = queueClient.GetQueueReference(queueName);
                cloudQueue.CreateIfNotExists();

                return cloudQueue;
            };
        }
    }
}
