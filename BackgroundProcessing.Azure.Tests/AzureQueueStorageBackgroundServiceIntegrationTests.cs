using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.Storage;
using Xunit;

namespace BackgroundProcessing.Azure.Tests
{
    public class AzureQueueStorageBackgroundServiceIntegrationTests
    {
        [Fact]
        public async Task ItShouldProcessBackgroundCommands()
        {
            StorageQueueIntegrationTestsCommandHandler.Commands.Clear();
            var commands = new[] { new StorageQueueIntegrationTestsCommand(), new StorageQueueIntegrationTestsCommand() };

            using (var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton(sp =>
                        {
                            var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                            var queueClient = storageAccount.CreateCloudQueueClient();
                            var queue = queueClient.GetQueueReference("bgtasks-integrationtests");
                            queue.DeleteIfExistsAsync().Wait();
                            queue.CreateIfNotExistsAsync().Wait();

                            return queue;
                        })
                        .AddAzureQueueStorageBackgroundDispatcher()
                        .AddAzureQueueStorageBackgroundProcessing()
                        .AddBackgroundCommandHandlersFromAssemblyContaining<AzureQueueStorageBackgroundServiceIntegrationTests>();
                })
                .Start())
            {
                var dispatcher = host.Services.GetRequiredService<IBackgroundDispatcher>();

                foreach (var command in commands)
                {
                    await dispatcher.DispatchAsync(command);
                }

                await Task.Delay(2000);
            }

            StorageQueueIntegrationTestsCommandHandler.Commands.Should().BeEquivalentTo(commands);
        }

        private class StorageQueueIntegrationTestsCommand : BackgroundCommand
        {
        }

        private class StorageQueueIntegrationTestsCommandHandler : IBackgroundCommandHandler<StorageQueueIntegrationTestsCommand>
        {
            public static readonly IList<StorageQueueIntegrationTestsCommand> Commands = new List<StorageQueueIntegrationTestsCommand>();

            public async Task HandleAsync(StorageQueueIntegrationTestsCommand command, CancellationToken cancellationToken = default)
            {
                Commands.Add(command);
            }
        }
    }
}
