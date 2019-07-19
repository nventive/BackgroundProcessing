using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BackgroundProcessing.Core.Testing
{
    /// <summary>
    /// <see cref="BackgroundBuilder"/> extension methods for testing.
    /// </summary>
    public static class BackgroundBuilderTestingExtensions
    {
        /// <summary>
        /// Adds a <see cref="GatedBackgroundProcessorDecorator"/> around the existing <see cref="IBackgroundProcessor"/>.
        /// Retrieves the instance of <see cref="GatedBackgroundProcessorAwaiter"/> to wait for events.
        /// </summary>
        /// <param name="builder">The <see cref="BackgroundBuilder"/>.</param>
        /// <param name="numberOfCommandsToWait">The number of commands to wait for.</param>
        public static void AddGatedBackgroundProcessorDecorator(this BackgroundBuilder builder, int numberOfCommandsToWait)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var wrappedDescriptor = builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IBackgroundProcessor));

            if (wrappedDescriptor == null)
            {
                throw new InvalidOperationException($"{typeof(IBackgroundProcessor).Name} is not registered.");
            }

            var intermediateProvider = builder.Services.BuildServiceProvider();

            builder.Services.AddSingleton(sp => new GatedBackgroundProcessorAwaiter(numberOfCommandsToWait));

            builder.Services.Replace(ServiceDescriptor.Describe(
              typeof(IBackgroundProcessor),
              sp => new GatedBackgroundProcessorDecorator(
                  intermediateProvider.GetRequiredService<IBackgroundProcessor>(),
                  sp.GetRequiredService<GatedBackgroundProcessorAwaiter>()),
              wrappedDescriptor.Lifetime));
        }
    }
}
