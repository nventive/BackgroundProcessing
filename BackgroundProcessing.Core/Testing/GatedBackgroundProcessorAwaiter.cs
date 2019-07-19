using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundProcessing.Core.Testing
{
    /// <summary>
    /// Allows waiting for commands to be processed.
    /// </summary>
    public sealed class GatedBackgroundProcessorAwaiter : IDisposable
    {
        private readonly CountdownEvent _countDownEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatedBackgroundProcessorAwaiter"/> class.
        /// </summary>
        /// <param name="numberOfCommandsToWaitFor">The number of commands to wait for before releasing.</param>
        public GatedBackgroundProcessorAwaiter(
            int numberOfCommandsToWaitFor)
        {
            _countDownEvent = new CountdownEvent(numberOfCommandsToWaitFor);
        }

        /// <summary>
        /// Wait for the number of commands to have occur.
        /// </summary>
        /// <param name="timeout">The absolute timeout to bail out.</param>
        public void Wait(TimeSpan timeout)
        {
            if (Debugger.IsAttached)
            {
                _countDownEvent.Wait();
                return;
            }

            if (!_countDownEvent.Wait(timeout))
            {
                throw new TaskCanceledException();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _countDownEvent?.Dispose();
        }

        /// <summary>
        /// Signals that an event occured.
        /// </summary>
        internal void Signal()
        {
            _countDownEvent.Signal();
        }
    }
}
