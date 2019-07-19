using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace BackgroundProcessing.Core.Tests
{
    public class HostingServiceIntegrationTests
    {
        private readonly ITestOutputHelper _output;

        public HostingServiceIntegrationTests(ITestOutputHelper output)
        {
            _output = output ?? throw new System.ArgumentNullException(nameof(output));
        }

        [Fact]
        public async Task ItShouldProcessBackgroundCommands()
        {
            HostingServiceIntegrationTestsCommandHandler.Commands.Clear();
            var commands = new[] { new HostingServiceIntegrationTestsCommand(), new HostingServiceIntegrationTestsCommand() };

            using (var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton<ILogger<ConcurrentQueueDispatcherBackgroundService>>(_ => new XunitLogger<ConcurrentQueueDispatcherBackgroundService>(_output));
                    services
                        .AddHostingServiceConcurrentQueueBackgroundProcessing()
                        .AddBackgroundCommandHandlersFromAssemblyContaining<HostingServiceIntegrationTests>();
                })
                .Start())
            {
                var dispatcher = host.Services.GetRequiredService<IBackgroundDispatcher>();

                foreach (var command in commands)
                {
                    await dispatcher.DispatchAsync(command);
                }

                await host.StopAsync();
            }

            HostingServiceIntegrationTestsCommandHandler.Commands.Should().BeEquivalentTo(commands);
        }

        private class HostingServiceIntegrationTestsCommand : BackgroundCommand
        {
        }

        private class HostingServiceIntegrationTestsCommandHandler : IBackgroundCommandHandler<HostingServiceIntegrationTestsCommand>
        {
            public static readonly IList<HostingServiceIntegrationTestsCommand> Commands = new List<HostingServiceIntegrationTestsCommand>();

            public async Task HandleAsync(HostingServiceIntegrationTestsCommand command, CancellationToken cancellationToken = default)
            {
                Commands.Add(command);
            }
        }
    }
}
