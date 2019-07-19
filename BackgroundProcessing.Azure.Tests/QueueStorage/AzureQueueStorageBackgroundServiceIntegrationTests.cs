using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.Storage;
using Xunit;

namespace BackgroundProcessing.Azure.Tests.QueueStorage
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
                await host.StopAsync();
            }

            StorageQueueIntegrationTestsCommandHandler.Commands.Should().BeEquivalentTo(commands);
        }

        [Fact]
        public async Task ItShouldInvokeErrorHandler()
        {
            var command = new StorageQueueIntegrationTestsErrorCommand();
            IBackgroundCommand caughtCommand = null;
            Exception caughtException = null;

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
                        .AddAzureQueueStorageBackgroundProcessing(options =>
                        {
                            options.ErrorHandler = async (cmd, ex, ct) =>
                            {
                                caughtCommand = cmd;
                                caughtException = ex;
                            };
                        })
                        .AddBackgroundCommandHandlersFromAssemblyContaining<AzureQueueStorageBackgroundServiceIntegrationTests>();
                })
                .Start())
            {
                var dispatcher = host.Services.GetRequiredService<IBackgroundDispatcher>();
                await dispatcher.DispatchAsync(command);
                await Task.Delay(1000);
                await host.StopAsync();
            }

            caughtCommand.Should().BeEquivalentTo(command);
            caughtException.Message.Should().Be(command.Id);
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

        private class StorageQueueIntegrationTestsErrorCommand : BackgroundCommand
        {
        }

        private class StorageQueueIntegrationTestsErrorCommandHandler : IBackgroundCommandHandler<StorageQueueIntegrationTestsErrorCommand>
        {
            public async Task HandleAsync(StorageQueueIntegrationTestsErrorCommand command, CancellationToken cancellationToken = default)
            {
                throw new Exception(command.Id);
            }
        }
    }
}
