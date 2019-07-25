using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core.Events;
using BackgroundProcessing.Core.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace BackgroundProcessing.Core.Tests.Events
{
    public class BackgroundCommandEventRepositoryIntegrationTests
    {
        [Fact]
        public async Task ItShouldProcessBackgroundCommands()
        {
            var commands = new[] { new BackgroundCommandEventRepositoryIntegrationTestsCommand(), new BackgroundCommandEventRepositoryIntegrationTestsCommand() };
            var repositoryMock = new Mock<IBackgroundCommandEventRepository>();

            using (var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton(sp => repositoryMock.Object)
                        .AddBackgroundCommandHandlersFromAssemblyContaining<ConcurrentQueueDispatcherBackgroundServiceIntegrationTests>()
                        .AddHostingServiceConcurrentQueueBackgroundProcessing()
                        .AddBackgroundCommandEventsRepositoryDecorators()
                        .AddCountdownEventBackgroundProcessorDecorator(commands.Count());
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

                foreach (var command in commands)
                {
                    foreach (var status in new[] { BackgroundCommandEventStatus.Dispatching, BackgroundCommandEventStatus.Processing, BackgroundCommandEventStatus.Processed })
                    {
                        repositoryMock.Verify(
                            x => x.Add(
                                It.Is<BackgroundCommandEvent>(evt => evt.Status == status && evt.Command == command),
                                It.IsAny<CancellationToken>()));
                    }
                }
            }
        }

        private class BackgroundCommandEventRepositoryIntegrationTestsCommand : BackgroundCommand
        {
        }

        private class BackgroundCommandEventRepositoryIntegrationTestsCommandHandler : IBackgroundCommandHandler<BackgroundCommandEventRepositoryIntegrationTestsCommand>
        {
            public async Task HandleAsync(BackgroundCommandEventRepositoryIntegrationTestsCommand command, CancellationToken cancellationToken = default)
            {
            }
        }
    }
}
