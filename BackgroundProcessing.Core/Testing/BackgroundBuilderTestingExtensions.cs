using System;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundProcessing.Core.Testing
{
    /// <summary>
    /// <see cref="BackgroundBuilder"/> extension methods for testing.
    /// </summary>
    public static class BackgroundBuilderTestingExtensions
    {
        /// <summary>
        /// Adds a <see cref="CountdownEventBackgroundProcessorDecorator"/> around the existing <see cref="IBackgroundProcessor"/>.
        /// Retrieves the instance of <see cref="CountdownEventBackgroundProcessorAwaiter"/> to wait for events.
        /// </summary>
        /// <param name="builder">The <see cref="BackgroundBuilder"/>.</param>
        /// <param name="numberOfCommandsToWait">The number of commands to wait for.</param>
        /// <returns>The configured <see cref="BackgroundBuilder"/>.</returns>
        public static BackgroundBuilder AddCountdownEventBackgroundProcessorDecorator(this BackgroundBuilder builder, int numberOfCommandsToWait)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton(sp => new CountdownEventBackgroundProcessorAwaiter(numberOfCommandsToWait));
            builder.DecorateProcessor<CountdownEventBackgroundProcessorDecorator>();

            return builder;
        }
    }
}
