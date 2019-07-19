using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using FluentAssertions;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
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
                        .AddSingleton(sp =>
                        {
                            var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                            var queueClient = storageAccount.CreateCloudQueueClient();
                            var queue = queueClient.GetQueueReference("bgtasks-integrationtests");
                            queue.DeleteIfExistsAsync().Wait();
                            queue.CreateIfNotExistsAsync().Wait();

                            return queue;
                        })
                        .AddBackgroundCommandHandlersFromAssemblyContaining<CloudQueueBackgroundServiceIntegrationTests>()
                        .AddAzureStorageQueueBackgroundDispatcher()
                        .Services
                        .AddAzureStorageQueueBackgroundProcessing();
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

            CloudQueueIntegrationTestsCommandHandler.Commands.Should().BeEquivalentTo(commands);
        }

        [Fact]
        public async Task ItShouldInvokeErrorHandler()
        {
            var command = new CloudQueueIntegrationTestsErrorCommand();
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
                        .AddBackgroundCommandHandlersFromAssemblyContaining<CloudQueueBackgroundServiceIntegrationTests>()
                        .AddAzureStorageQueueBackgroundDispatcher()
                        .Services
                        .AddAzureStorageQueueBackgroundProcessing(options =>
                        {
                            options.ErrorHandler = async (cmd, ex, ct) =>
                            {
                                caughtCommand = cmd;
                                caughtException = ex;
                            };
                        });
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

        private class CloudQueueIntegrationTestsCommand : BackgroundCommand
        {
        }

        private class CloudQueueIntegrationTestsCommandHandler : IBackgroundCommandHandler<CloudQueueIntegrationTestsCommand>
        {
            public static readonly IList<CloudQueueIntegrationTestsCommand> Commands = new List<CloudQueueIntegrationTestsCommand>();

            public async Task HandleAsync(CloudQueueIntegrationTestsCommand command, CancellationToken cancellationToken = default)
            {
                Commands.Add(command);
            }
        }

        private class CloudQueueIntegrationTestsErrorCommand : BackgroundCommand
        {
        }

        private class CloudQueueIntegrationTestsErrorCommandHandler : IBackgroundCommandHandler<CloudQueueIntegrationTestsErrorCommand>
        {
            public async Task HandleAsync(CloudQueueIntegrationTestsErrorCommand command, CancellationToken cancellationToken = default)
            {
                throw new Exception(command.Id);
            }
        }
    }
}
