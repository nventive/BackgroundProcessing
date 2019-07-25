using System;
using BackgroundProcessing.Azure.Storage.Table;
using BackgroundProcessing.Core;
using BackgroundProcessing.Core.Events;
using BackgroundProcessing.Core.Serializers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="BackgroundBuilder"/> extension methods.
    /// </summary>
    public static class BackgroundBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="CloudTableBackgroundCommandEventRepository"/> to store events using <see cref="CloudTable"/>.
        /// </summary>
        /// <param name="builder">The <see cref="BackgroundBuilder"/>.</param>
        /// <param name="cloudTableProvider">The <see cref="CloudTable"/> provider.</param>
        /// <returns>The configured <see cref="BackgroundBuilder"/>.</returns>
        public static BackgroundBuilder AddCloudTableEventRepository(
            this BackgroundBuilder builder,
            Func<IServiceProvider, CloudTable> cloudTableProvider)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (cloudTableProvider is null)
            {
                throw new ArgumentNullException(nameof(cloudTableProvider));
            }

            builder.Services.TryAddSingleton<IBackgroundCommandSerializer, JsonNetBackgroundCommandSerializer>();
            builder.Services.AddSingleton<IBackgroundCommandEventRepository>(
                sp => new CloudTableBackgroundCommandEventRepository(
                    cloudTableProvider(sp),
                    sp.GetRequiredService<IBackgroundCommandSerializer>()));
            return builder;
        }
    }
}
