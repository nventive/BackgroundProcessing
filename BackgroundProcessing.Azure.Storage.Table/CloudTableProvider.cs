using System;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Helper class to initialize <see cref="CloudTable"/> instances.
    /// </summary>
    public static class CloudTableProvider
    {
        /// <summary>
        /// Returns a memoized <see cref="CloudTable"/> provider initialized from a connection string name and a table name.
        /// Will create the table if it does not exists.
        /// </summary>
        /// <param name="connectionStringName">The name of the connection string.</param>
        /// <param name="tableName">The storage table name.</param>
        /// <returns>The <see cref="CloudTable"/> provider.</returns>
        public static Func<IServiceProvider, CloudTable> FromConnectionStringName(
            string connectionStringName,
            string tableName)
        {
            CloudTable memoizedTable = null;
            var objectLock = new object();

            return (sp) =>
            {
                if (memoizedTable is null)
                {
                    lock (objectLock)
                    {
                        if (memoizedTable is null)
                        {
                            var configuration = sp.GetRequiredService<IConfiguration>();
                            var connectionString = configuration.GetConnectionString(connectionStringName);
                            var storageAccount = CloudStorageAccount.Parse(connectionString);
                            var tableClient = storageAccount.CreateCloudTableClient();
                            var cloudTable = tableClient.GetTableReference(tableName);
                            cloudTable.CreateIfNotExists();
                            memoizedTable = cloudTable;
                        }
                    }
                }

                return memoizedTable;
            };
        }

        /// <summary>
        /// Returns a memoized <see cref="CloudTable"/> provider initialized from a connection string and a table name.
        /// Will create the table if it does not exists.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="tableName">The storage table name.</param>
        /// <returns>The <see cref="CloudTable"/> provider.</returns>
        public static Func<IServiceProvider, CloudTable> FromConnectionString(
            string connectionString,
            string tableName)
        {
            CloudTable memoizedTable = null;
            var objectLock = new object();

            return (sp) =>
            {
                if (memoizedTable is null)
                {
                    lock (objectLock)
                    {
                        if (memoizedTable is null)
                        {
                            var storageAccount = CloudStorageAccount.Parse(connectionString);
                            var tableClient = storageAccount.CreateCloudTableClient();
                            var cloudTable = tableClient.GetTableReference(tableName);
                            cloudTable.CreateIfNotExists();
                            memoizedTable = cloudTable;
                        }
                    }
                }

                return memoizedTable;
            };
        }
    }
}
