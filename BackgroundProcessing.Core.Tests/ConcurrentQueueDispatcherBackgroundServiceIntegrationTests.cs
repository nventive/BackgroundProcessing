using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            HostingServiceIntegrationTestsCommandHandler.Commands.Clear();
            var commands = new[] { new HostingServiceIntegrationTestsCommand(), new HostingServiceIntegrationTestsCommand() };

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

                HostingServiceIntegrationTestsCommandHandler.Commands.Should().HaveCount(commands.Count());
            }
        }

        [Fact]
        public async Task ItShouldInvokeErrorHandler()
        {
            var command = new HostingServiceIntegrationTestsErrorCommand();
            IBackgroundCommand caughtCommand = null;
            Exception caughtException = null;

            using (var countDownEvent = new CountdownEvent(1))
            using (var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton(sp => _output)
                        .AddSingleton<ILogger<ConcurrentQueueDispatcherBackgroundService>>(_ => new XunitLogger<ConcurrentQueueDispatcherBackgroundService>(_output));
                    services
                        .AddBackgroundCommandHandlersFromAssemblyContaining<ConcurrentQueueDispatcherBackgroundServiceIntegrationTests>()
                        .AddHostingServiceConcurrentQueueBackgroundProcessing(options =>
                        {
                            options.ErrorHandler = async (cmd, ex, ct) =>
                            {
                                caughtCommand = cmd;
                                caughtException = ex;
                                countDownEvent.Signal();
                            };
                        });
                })
                .Start())
            {
                var dispatcher = host.Services.GetRequiredService<IBackgroundDispatcher>();
                await dispatcher.DispatchAsync(command);
                countDownEvent.Wait(TimeSpan.FromSeconds(30));

                caughtCommand.Should().Be(command);
                caughtException.Message.Should().Be(command.Id);
            }
        }

        private class HostingServiceIntegrationTestsCommand : BackgroundCommand
        {
        }

        private class HostingServiceIntegrationTestsCommandHandler : IBackgroundCommandHandler<HostingServiceIntegrationTestsCommand>
        {
            public static readonly ConcurrentBag<HostingServiceIntegrationTestsCommand> Commands = new ConcurrentBag<HostingServiceIntegrationTestsCommand>();
            private readonly ITestOutputHelper _output;

            public HostingServiceIntegrationTestsCommandHandler(ITestOutputHelper output)
            {
                _output = output ?? throw new ArgumentNullException(nameof(output));
            }

            public async Task HandleAsync(HostingServiceIntegrationTestsCommand command, CancellationToken cancellationToken = default)
            {
                _output.WriteLine($"Processing {command}");
                Commands.Add(command);
                _output.WriteLine($"Processed {command}");
            }
        }

        private class HostingServiceIntegrationTestsErrorCommand : BackgroundCommand
        {
        }

        private class HostingServiceIntegrationTestsErrorCommandHandler : IBackgroundCommandHandler<HostingServiceIntegrationTestsErrorCommand>
        {
            private readonly ITestOutputHelper _output;

            public HostingServiceIntegrationTestsErrorCommandHandler(ITestOutputHelper output)
            {
                _output = output ?? throw new ArgumentNullException(nameof(output));
            }

            public async Task HandleAsync(HostingServiceIntegrationTestsErrorCommand command, CancellationToken cancellationToken = default)
            {
                _output.WriteLine($"Throw exception: {command}");
                throw new Exception(command.Id);
            }
        }
    }
}
