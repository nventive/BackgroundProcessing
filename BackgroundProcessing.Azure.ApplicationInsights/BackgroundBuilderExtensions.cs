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
        /// <returns>The configured <see cref="BackgroundBuilder"/>.</returns>
        public static BackgroundBuilder AddApplicationInsightsDecorators(this BackgroundBuilder builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.TryDecorateDispatcher<TelemetryClientDispatcherDecorator>();
            builder.TryDecorateProcessor<TelemetryClientProcessorDecorator>();
            return builder;
        }
    }
}
