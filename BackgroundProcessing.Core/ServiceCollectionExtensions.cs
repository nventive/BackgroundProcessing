using System.Linq;
using System.Reflection;
using BackgroundProcessing.Core;
using BackgroundProcessing.Core.HostingService;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all <see cref="IBackgroundCommandHandler{TCommand}"/> in the assembly containing <typeparamref name="T"/>
        /// with <paramref name="lifetime"/>.
        /// </summary>
        /// <typeparam name="T">The type that specifies the assembly to scan.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> to register with.</param>
        /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddBackgroundCommandHandlersFromAssemblyContaining<T>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
            => services.AddBackgroundCommandHandlersFromAssembly(typeof(T).Assembly, lifetime);

        /// <summary>
        /// Registers all <see cref="IBackgroundCommandHandler{TCommand}"/> in <paramref name="assembly"/> with <paramref name="lifetime"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="assembly">The <see cref="Assembly"/> to scan.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> to register with.</param>
        /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddBackgroundCommandHandlersFromAssembly(this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (services == null)
            {
                throw new System.ArgumentNullException(nameof(services));
            }

            if (assembly == null)
            {
                throw new System.ArgumentNullException(nameof(assembly));
            }

            var handlerTypes = assembly
                .GetTypes()
                .Where(x => !x.IsAbstract && !x.IsGenericTypeDefinition && x.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IBackgroundCommandHandler<>)));

            foreach (var handlerType in handlerTypes)
            {
                var serviceTypes = handlerType
                    .GetInterfaces()
                    .Where(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IBackgroundCommandHandler<>));

                foreach (var serviceType in serviceTypes)
                {
                    services.Add(new ServiceDescriptor(serviceType, handlerType, lifetime));
                }
            }

            return services;
        }

        /// <summary>
        /// Setup local processing of <see cref="IBackgroundCommand"/> using in-memory queue.
        /// Use only for POCs or unit-test, as it does not provide any retry or persistence mechanism.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddHostingServiceQueueBackgroundProcessing(this IServiceCollection services)
        {
            services.TryAddSingleton<IBackgroundCommandQueue, ConcurrentBackgroundCommandQueueDispatcher>();
            services.TryAddScoped<IBackgroundProcessor, ServiceProviderBackgroundProcessor>();
            services.TryAddScoped<IBackgroundDispatcher, BackgroundCommandQueueDispatcher>();
            services.AddHostedService<BackgroundCommandQueueService>();

            return services;
        }
    }
}
