using System;
using BackgroundProcessing.Caching;
using BackgroundProcessing.Core.Events;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="BackgroundBuilder"/> extension methods.
    /// </summary>
    public static class BackgroundBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="MemoryCacheBackgroundCommandEventRepository"/> to store events using <see cref="IMemoryCache"/>.
        /// Do not forget to register a <see cref="IMemoryCache"/> implementation.
        /// </summary>
        /// <param name="builder">The <see cref="BackgroundBuilder"/>.</param>
        /// <param name="expiration">The expiration time of events. Defaults to 10 minutes.</param>
        /// <returns>The configured <see cref="BackgroundBuilder"/>.</returns>
        public static BackgroundBuilder AddMemoryCacheEventRepository(this BackgroundBuilder builder, TimeSpan? expiration = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<IBackgroundCommandEventRepository>(
                sp => new MemoryCacheBackgroundCommandEventRepository(
                    sp.GetRequiredService<IMemoryCache>(),
                    expiration.HasValue ? expiration.Value : TimeSpan.FromMinutes(10)));
            return builder;
        }

        /// <summary>
        /// Adds <see cref="DistributedCacheBackgroundCommandEventRepository"/> to store events using <see cref="IDistributedCache"/>.
        /// Do not forget to register a <see cref="IDistributedCache"/> implementation.
        /// </summary>
        /// <param name="builder">The <see cref="BackgroundBuilder"/>.</param>
        /// <param name="expiration">The expiration time of events. Defaults to 10 minutes.</param>
        /// <returns>The configured <see cref="BackgroundBuilder"/>.</returns>
        public static BackgroundBuilder AddDistributedCacheEventRepository(this BackgroundBuilder builder, TimeSpan? expiration = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<IBackgroundCommandEventRepository>(
                sp => new DistributedCacheBackgroundCommandEventRepository(
                    sp.GetRequiredService<IDistributedCache>(),
                    expiration.HasValue ? expiration.Value : TimeSpan.FromMinutes(10),
                    sp.GetService<IBackgroundCommandEventsSerializer>()));
            return builder;
        }
    }
}
