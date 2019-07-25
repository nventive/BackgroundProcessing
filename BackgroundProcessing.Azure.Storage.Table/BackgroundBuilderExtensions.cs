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
        /// <param name="cloudTable">The <see cref="CloudTable"/> instance to use.</param>
        /// <returns>The configured <see cref="BackgroundBuilder"/>.</returns>
        public static BackgroundBuilder AddCloudTableEventRepository(this BackgroundBuilder builder, CloudTable cloudTable)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddSingleton<IBackgroundCommandSerializer, JsonNetBackgroundCommandSerializer>();
            builder.Services.AddSingleton<IBackgroundCommandEventRepository>(
                sp => new CloudTableBackgroundCommandEventRepository(
                    cloudTable,
                    sp.GetRequiredService<IBackgroundCommandSerializer>()));
            return builder;
        }
    }
}
