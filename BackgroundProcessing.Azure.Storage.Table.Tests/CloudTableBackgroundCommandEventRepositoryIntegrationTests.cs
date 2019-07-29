using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using BackgroundProcessing.Core.Events;
using BackgroundProcessing.Core.Testing;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace BackgroundProcessing.Azure.Storage.Queue.Tests
{
    public class CloudTableBackgroundCommandEventRepositoryIntegrationTests
    {
        [Fact]
        public async Task ItShouldProcessBackgroundCommands()
        {
            var commands = new[] { new CloudTableBackgroundCommandEventRepositoryIntegrationTestsCommand(), new CloudTableBackgroundCommandEventRepositoryIntegrationTestsCommand() };

            using (var host = new HostBuilder()
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    config
                        .AddJsonFile("appsettings.json", true)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices(services =>
                {
                    services
                        .AddBackgroundCommandHandlersFromAssemblyContaining<CloudTableBackgroundCommandEventRepositoryIntegrationTests>()
                        .AddHostingServiceConcurrentQueueBackgroundProcessing()
                        .AddBackgroundCommandEventsRepositoryDecorators()
                        .AddCountdownEventBackgroundProcessorDecorator(commands.Count())
                        .AddCloudTableEventRepository(CloudTableProvider.FromConnectionStringName("StorageTable", "bgtasksintegrationtests"));
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
                allEvents.Should().OnlyContain(x => x.Command.Id.Equals(commands[0].Id, StringComparison.Ordinal));

                var latestEvent = await repository.GetLatestEventForCommandId(commands[0].Id);
                latestEvent.Command.Id.Should().Be(commands[0].Id);
                latestEvent.Status.Should().Be(BackgroundCommandEventStatus.Processed);
            }
        }

        private class CloudTableBackgroundCommandEventRepositoryIntegrationTestsCommand : BackgroundCommand
        {
        }

        private class CloudTableBackgroundCommandEventRepositoryIntegrationTestsCommandHandler : IBackgroundCommandHandler<CloudTableBackgroundCommandEventRepositoryIntegrationTestsCommand>
        {
            public async Task HandleAsync(CloudTableBackgroundCommandEventRepositoryIntegrationTestsCommand command, CancellationToken cancellationToken = default)
            {
            }
        }
    }
}
