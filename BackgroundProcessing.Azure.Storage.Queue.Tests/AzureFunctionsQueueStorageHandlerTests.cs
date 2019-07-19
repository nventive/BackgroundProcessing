using System;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using BackgroundProcessing.Core.Serializers;
using Moq;
using Xunit;

namespace BackgroundProcessing.Azure.Storage.Queue.Tests
{
    public class AzureFunctionsQueueStorageHandlerTests
    {
        [Fact]
        public async Task ItShouldProcessCommands()
        {
            var command = new AzureFunctionsQueueStorageHandlerTestsCommand();

            var serializer = new JsonNetBackgroundCommandSerializer();
            var processorMock = new Mock<IBackgroundProcessor>();
            var functionsHandler = new AzureFunctionsQueueStorageHandler(serializer, processorMock.Object);

            var message = await serializer.SerializeAsync(command);

            await functionsHandler.HandleAsync(message);

            processorMock.Verify(
                x => x.ProcessAsync(
                    It.Is<IBackgroundCommand>(y => y.Id.Equals(command.Id, StringComparison.Ordinal)),
                    It.IsAny<CancellationToken>()));
        }

        private class AzureFunctionsQueueStorageHandlerTestsCommand : BackgroundCommand
        {
        }
    }
}
