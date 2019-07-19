using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using BackgroundProcessing.Core.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace BackgroundProcessing.Azure.Storage.Queue.Tests
{
    public class CloudQueueBackgroundServiceIntegrationTests
    {
        [Fact]
        public async Task ItShouldProcessBackgroundCommands()
        {
            CloudQueueIntegrationTestsCommandHandler.Commands.Clear();
            var commands = new[] { new CloudQueueIntegrationTestsCommand(), new CloudQueueIntegrationTestsCommand() };

            using (var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddBackgroundCommandHandlersFromAssemblyContaining<CloudQueueBackgroundServiceIntegrationTests>()
                        .AddAzureStorageQueueBackgroundDispatcher()
                        .Services
                        .AddAzureStorageQueueBackgroundProcessing()
                        .ConfigureCloudQueueUsingConnectionString("UseDevelopmentStorage=true", "bgtasks-integrationtests")
                        .AddGatedBackgroundProcessorDecorator(commands.Count());
                })
                .Start())
            {
                var dispatcher = host.Services.GetRequiredService<IBackgroundDispatcher>();

                foreach (var command in commands)
                {
                    await dispatcher.DispatchAsync(command);
                }

                var awaiter = host.Services.GetRequiredService<GatedBackgroundProcessorAwaiter>();
                awaiter.Wait(TimeSpan.FromSeconds(30));

                CloudQueueIntegrationTestsCommandHandler.Commands.Should().HaveCount(commands.Count());
            }
        }

        private class CloudQueueIntegrationTestsCommand : BackgroundCommand
        {
        }

        private class CloudQueueIntegrationTestsCommandHandler : IBackgroundCommandHandler<CloudQueueIntegrationTestsCommand>
        {
            public static readonly ConcurrentBag<CloudQueueIntegrationTestsCommand> Commands = new ConcurrentBag<CloudQueueIntegrationTestsCommand>();

            public async Task HandleAsync(CloudQueueIntegrationTestsCommand command, CancellationToken cancellationToken = default)
            {
                Commands.Add(command);
            }
        }
    }
}
