using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BackgroundProcessing.Core.Tests
{
    public class ServiceProviderBackgroundProcessorTests
    {
        [Fact]
        public async Task ItShouldThrowIfNoHandlerCanBeFound()
        {
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var command = new TestCommand();
            var processor = new ServiceProviderBackgroundProcessor(serviceProvider);

            Func<Task> act = async () => await processor.ProcessAsync(command);

            act.Should().Throw<BackgroundProcessingException>().WithMessage("*handler*TestCommand*");
        }

        [Fact]
        public async Task ItShouldInvokeHandlerWhenFound()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IBackgroundCommandHandler<TestCommand>, TestCommandHandler>()
                .BuildServiceProvider();
            var command = new TestCommand();
            var processor = new ServiceProviderBackgroundProcessor(serviceProvider);

            await processor.ProcessAsync(command);

            var handler = serviceProvider.GetRequiredService<IBackgroundCommandHandler<TestCommand>>() as TestCommandHandler;
            handler.ReceivedCommand.Should().BeSameAs(command);
        }

        private class TestCommand : BackgroundCommand
        {
        }

        private class TestCommandHandler : IBackgroundCommandHandler<TestCommand>
        {
            public TestCommand ReceivedCommand { get; private set; }

            public async Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
            {
                ReceivedCommand = command;
            }
        }
    }
}
