using BackgroundProcessing.Core;
using BackgroundProcessing.Core.Events;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="BackgroundBuilder"/> extension methods.
    /// </summary>
    public static class BackgroundBuilderEventsExtensions
    {
        /// <summary>
        /// Adds dispatcher and processor decorators that sends events to a <see cref="IBackgroundCommandEventRepository"/>.
        /// Do NOT forget to also register a <see cref="IBackgroundCommandEventRepository"/> service for this to work properly.
        /// </summary>
        /// <param name="builder">The <see cref="BackgroundBuilder"/>.</param>
        /// <returns>The configured <see cref="BackgroundBuilder"/>.</returns>
        public static BackgroundBuilder AddBackgroundCommandEventsRepositoryDecorators(this BackgroundBuilder builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.TryDecorateDispatcher<BackgroundCommandEventRepositoryDispatcherDecorator>();
            builder.TryDecorateProcessor<BackgroundCommandEventRepositoryProcessorDecorator>();
            return builder;
        }
    }
}
