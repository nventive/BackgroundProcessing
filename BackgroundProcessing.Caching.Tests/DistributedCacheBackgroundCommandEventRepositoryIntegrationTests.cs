using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using BackgroundProcessing.Core.Events;
using BackgroundProcessing.Core.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace BackgroundProcessing.Caching.Tests
{
    public class DistributedCacheBackgroundCommandEventRepositoryIntegrationTests
    {
        [Fact]
        public async Task ItShouldProcessBackgroundCommands()
        {
            var commands = new[] { new DistributedCacheBackgroundCommandEventRepositoryIntegrationTestsCommand() }; // new DistributedCacheBackgroundCommandEventRepositoryIntegrationTestsCommand() };

            using (var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddDistributedMemoryCache()
                        .AddBackgroundCommandHandlersFromAssemblyContaining<MemoryCacheBackgroundCommandEventRepositoryIntegrationTests>()
                        .AddHostingServiceConcurrentQueueBackgroundProcessing()
                        .AddBackgroundCommandEventsRepositoryDecorators()
                        .AddCountdownEventBackgroundProcessorDecorator(commands.Count())
                        .AddDistributedCacheEventRepository();
                })
                .Start())
            {
                var dispatcher = host.Services.GetRequiredService<IBackgroundDispatcher>();

                foreach (var command in commands)
                {
                    await dispatcher.DispatchAsync(command);
                }

                var awaiter = host.Services.GetRequiredService<CountdownEventBackgroundProcessorAwaiter>();
                awaiter.Wait(TimeSpan.FromSeconds(30));

                var repository = host.Services.GetRequiredService<IBackgroundCommandEventRepository>();
                var allEvents = await repository.GetAllEventsForCommandId(commands[0].Id);
                allEvents.Should().NotBeEmpty();
                allEvents.Should().HaveCountGreaterOrEqualTo(2); // Dispatched can take more time to appear, so we can avoid checking for it.
                allEvents.Should().OnlyContain(x => x.Command.Id.Equals(commands[0].Id, StringComparison.Ordinal));

                var latestEvent = await repository.GetLatestEventForCommandId(commands[0].Id);
                latestEvent.Command.Id.Should().Be(commands[0].Id);
                latestEvent.Status.Should().Be(BackgroundCommandEventStatus.Processed);
            }
        }

        private class DistributedCacheBackgroundCommandEventRepositoryIntegrationTestsCommand : BackgroundCommand
        {
        }

        private class DistributedCacheBackgroundCommandEventRepositoryIntegrationTestsCommandHandler : IBackgroundCommandHandler<DistributedCacheBackgroundCommandEventRepositoryIntegrationTestsCommand>
        {
            public async Task HandleAsync(DistributedCacheBackgroundCommandEventRepositoryIntegrationTestsCommand command, CancellationToken cancellationToken = default)
            {
            }
        }
    }
}
