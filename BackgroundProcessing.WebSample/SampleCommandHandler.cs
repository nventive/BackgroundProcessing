using System;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;

namespace BackgroundProcessing.WebSample
{
    public class SampleCommandHandler : IBackgroundCommandHandler<SampleCommand>
    {
        public async Task HandleAsync(SampleCommand command, CancellationToken cancellationToken = default)
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    }
}
