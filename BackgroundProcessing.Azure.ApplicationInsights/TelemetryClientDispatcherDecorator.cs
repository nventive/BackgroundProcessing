using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace BackgroundProcessing.Azure.ApplicationInsights
{
    /// <summary>
    /// <see cref="IBackgroundDispatcher"/> decorator that adds Application Insights support.
    /// </summary>
    public class TelemetryClientDispatcherDecorator : IBackgroundDispatcher
    {
        /// <summary>
        /// The name of a successful dispatched event.
        /// </summary>
        public const string BackgroundCommandDispatchedEventName = "BackgroundCommandDispatched";

        /// <summary>
        /// The name of the Application Insights Property that gets the <see cref="IBackgroundCommand.Id"/> value.
        /// </summary>
        public const string BackgroundCommandIdPropertyName = "BackgroundCommandId";

        /// <summary>
        /// The name of the metric that aggregates dispatch time.
        /// </summary>
        public const string BackgroundCommandDispatchTimeMetricName = "BackgroundCommandDispatchTime";

        private readonly IBackgroundDispatcher _wrappedDispatcher;
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryClientDispatcherDecorator"/> class.
        /// </summary>
        /// <param name="wrappedDispatcher">The inner <see cref="IBackgroundDispatcher"/>.</param>
        /// <param name="telemetryClient">The <see cref="TelemetryClient"/>.</param>
        public TelemetryClientDispatcherDecorator(
            IBackgroundDispatcher wrappedDispatcher,
            TelemetryClient telemetryClient)
        {
            _wrappedDispatcher = wrappedDispatcher ?? throw new ArgumentNullException(nameof(wrappedDispatcher));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        /// <inheritdoc />
        public async Task DispatchAsync(IBackgroundCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            try
            {
                var stopwatch = Stopwatch.StartNew();
                await _wrappedDispatcher.DispatchAsync(command, cancellationToken);
                stopwatch.Stop();
                var eventTelemetry = new EventTelemetry(BackgroundCommandDispatchedEventName);
                eventTelemetry.Properties.Add(BackgroundCommandIdPropertyName, command.Id);
                eventTelemetry.Metrics.Add(BackgroundCommandDispatchTimeMetricName, stopwatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent(eventTelemetry);
            }
            catch (Exception ex)
            {
                var exceptionTelemetry = new ExceptionTelemetry(ex);
                exceptionTelemetry.Properties.Add(BackgroundCommandIdPropertyName, command.Id);
                _telemetryClient.TrackException(exceptionTelemetry);
                throw;
            }
        }
    }
}
