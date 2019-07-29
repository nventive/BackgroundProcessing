using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace BackgroundProcessing.Core.Tests
{
    public class ConcurrentQueueDispatcherBackgroundServiceIntegrationTests
    {
        private readonly ITestOutputHelper _output;

        public ConcurrentQueueDispatcherBackgroundServiceIntegrationTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        [Fact]
        public async Task ItShouldProcessBackgroundCommands()
        {
            ConcurrentQueueIntegrationTestsCommandHandler.Commands.Clear();
            var commands = new[] { new ConcurrentQueueIntegrationTestsCommand(), new ConcurrentQueueIntegrationTestsCommand() };

            using (var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton(sp => _output)
                        .AddSingleton<ILogger<ConcurrentQueueDispatcherBackgroundService>>(_ => new XunitLogger<ConcurrentQueueDispatcherBackgroundService>(_output));
                    services
                        .AddBackgroundCommandHandlersFromAssemblyContaining<ConcurrentQueueDispatcherBackgroundServiceIntegrationTests>()
                        .AddHostingServiceConcurrentQueueBackgroundProcessing()
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

                ConcurrentQueueIntegrationTestsCommandHandler.Commands.Should().HaveCount(commands.Count());
            }
        }

        private class ConcurrentQueueIntegrationTestsCommand : BackgroundCommand
        {
        }

        private class ConcurrentQueueIntegrationTestsCommandHandler : IBackgroundCommandHandler<ConcurrentQueueIntegrationTestsCommand>
        {
            public static readonly ConcurrentBag<ConcurrentQueueIntegrationTestsCommand> Commands = new ConcurrentBag<ConcurrentQueueIntegrationTestsCommand>();
            private readonly ITestOutputHelper _output;

            public ConcurrentQueueIntegrationTestsCommandHandler(ITestOutputHelper output)
            {
                _output = output ?? throw new ArgumentNullException(nameof(output));
            }

            public async Task HandleAsync(ConcurrentQueueIntegrationTestsCommand command, CancellationToken cancellationToken = default)
            {
                _output.WriteLine($"Processing {command}");
                Commands.Add(command);
                _output.WriteLine($"Processed {command}");
            }
        }
    }
}
