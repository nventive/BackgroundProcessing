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
    public class MemoryCacheBackgroundCommandEventRepositoryIntegrationTests
    {
        [Fact]
        public async Task ItShouldProcessBackgroundCommands()
        {
            var commands = new[] { new MemoryCacheBackgroundCommandEventRepositoryIntegrationTestsCommand(), new MemoryCacheBackgroundCommandEventRepositoryIntegrationTestsCommand() };

            using (var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddMemoryCache()
                        .AddBackgroundCommandHandlersFromAssemblyContaining<MemoryCacheBackgroundCommandEventRepositoryIntegrationTests>()
                        .AddHostingServiceConcurrentQueueBackgroundProcessing()
                        .AddBackgroundCommandEventsRepositoryDecorators()
                        .AddCountdownEventBackgroundProcessorDecorator(commands.Count())
                        .AddMemoryCacheEventRepository();
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
                allEvents.Should().HaveCount(3);
                allEvents.Should().OnlyContain(x => x.Command == commands[0]);
            }
        }

        private class MemoryCacheBackgroundCommandEventRepositoryIntegrationTestsCommand : BackgroundCommand
        {
        }

        private class MemoryCacheBackgroundCommandEventRepositoryIntegrationTestsCommandHandler : IBackgroundCommandHandler<MemoryCacheBackgroundCommandEventRepositoryIntegrationTestsCommand>
        {
            public async Task HandleAsync(MemoryCacheBackgroundCommandEventRepositoryIntegrationTestsCommand command, CancellationToken cancellationToken = default)
            {
            }
        }
    }
}
