using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;

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
        /// The name of the metric that aggregates dispatch time.
        /// </summary>
        public const string BackgroundCommandDispatchTimeMetricName = "BackgroundCommandDispatchTime";

        private readonly IBackgroundDispatcher _wrappedDispatcher;
        private readonly IOptions<TelemetryClientDecoratorOptions> _options;
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryClientDispatcherDecorator"/> class.
        /// </summary>
        /// <param name="wrappedDispatcher">The inner <see cref="IBackgroundDispatcher"/>.</param>
        /// <param name="options">The <see cref="TelemetryClientDecoratorOptions"/>.</param>
        /// <param name="telemetryClient">The <see cref="TelemetryClient"/>.</param>
        public TelemetryClientDispatcherDecorator(
            IBackgroundDispatcher wrappedDispatcher,
            IOptions<TelemetryClientDecoratorOptions> options,
            TelemetryClient telemetryClient)
        {
            _wrappedDispatcher = wrappedDispatcher ?? throw new ArgumentNullException(nameof(wrappedDispatcher));
            _options = options ?? throw new ArgumentNullException(nameof(options));
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
                eventTelemetry.Properties.Add(nameof(command.Id), command.Id);
                eventTelemetry.Properties.Add(nameof(command.Timestamp), command.Timestamp.ToString("O", CultureInfo.InvariantCulture));
                eventTelemetry.Metrics.Add(BackgroundCommandDispatchTimeMetricName, stopwatch.ElapsedMilliseconds);
                _options.Value.AdditionalProperties?.Invoke(command, eventTelemetry.Properties);
                _telemetryClient.TrackEvent(eventTelemetry);
            }
            catch (Exception ex)
            {
                var exceptionTelemetry = new ExceptionTelemetry(ex);
                exceptionTelemetry.Properties.Add(nameof(command.Id), command.Id);
                exceptionTelemetry.Properties.Add(nameof(command.Timestamp), command.Timestamp.ToString("O", CultureInfo.InvariantCulture));
                _options.Value.AdditionalProperties?.Invoke(command, exceptionTelemetry.Properties);
                _telemetryClient.TrackException(exceptionTelemetry);
                throw;
            }
        }
    }
}
