using System;
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
            HostingServiceIntegrationTestsCommandHandler.Commands.Clear();
            var commands = new[] { new HostingServiceIntegrationTestsCommand(), new HostingServiceIntegrationTestsCommand() };

            using (var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton<ILogger<ConcurrentQueueDispatcherBackgroundService>>(_ => new XunitLogger<ConcurrentQueueDispatcherBackgroundService>(_output));
                    services
                        .AddHostingServiceConcurrentQueueBackgroundProcessing()
                        .AddBackgroundCommandHandlersFromAssemblyContaining<ConcurrentQueueDispatcherBackgroundServiceIntegrationTests>();
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

        [Fact]
        public async Task ItShouldInvokeErrorHandler()
        {
            var command = new HostingServiceIntegrationTestsErrorCommand();
            IBackgroundCommand caughtCommand = null;
            Exception caughtException = null;
            using (var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton<ILogger<ConcurrentQueueDispatcherBackgroundService>>(_ => new XunitLogger<ConcurrentQueueDispatcherBackgroundService>(_output));
                    services
                        .AddHostingServiceConcurrentQueueBackgroundProcessing(options =>
                        {
                            options.ErrorHandler = async (cmd, ex, ct) =>
                            {
                                caughtCommand = cmd;
                                caughtException = ex;
                            };
                        })
                        .AddBackgroundCommandHandlersFromAssemblyContaining<ConcurrentQueueDispatcherBackgroundServiceIntegrationTests>();
                })
                .Start())
            {
                var dispatcher = host.Services.GetRequiredService<IBackgroundDispatcher>();
                await dispatcher.DispatchAsync(command);
                await Task.Delay(200);
                await host.StopAsync();
            }

            caughtCommand.Should().Be(command);
            caughtException.Message.Should().Be(command.Id);
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

        private class HostingServiceIntegrationTestsErrorCommand : BackgroundCommand
        {
        }

        private class HostingServiceIntegrationTestsErrorCommandHandler : IBackgroundCommandHandler<HostingServiceIntegrationTestsErrorCommand>
        {
            public async Task HandleAsync(HostingServiceIntegrationTestsErrorCommand command, CancellationToken cancellationToken = default)
            {
                throw new Exception(command.Id);
            }
        }
    }
}
