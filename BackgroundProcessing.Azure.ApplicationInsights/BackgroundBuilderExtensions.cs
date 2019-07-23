using System;
using BackgroundProcessing.Azure.ApplicationInsights;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="BackgroundBuilder"/> extension methods.
    /// </summary>
    public static class BackgroundBuilderExtensions
    {
        /// <summary>
        /// Adds Application Insights monitoring to background processing.
        /// </summary>
        /// <param name="builder">The <see cref="BackgroundBuilder"/>.</param>
        /// <param name="configureOptions">Configure the <see cref="TelemetryClientDecoratorOptions"/> if needed.</param>
        /// <returns>The configured <see cref="BackgroundBuilder"/>.</returns>
        public static BackgroundBuilder AddApplicationInsightsDecorators(this BackgroundBuilder builder, Action<TelemetryClientDecoratorOptions> configureOptions)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            if (configureOptions != null)
            {
                builder.Services.Configure(configureOptions);
            }

            builder.TryDecorateDispatcher<TelemetryClientDispatcherDecorator>();
            builder.TryDecorateProcessor<TelemetryClientProcessorDecorator>();
            return builder;
        }
    }
}
