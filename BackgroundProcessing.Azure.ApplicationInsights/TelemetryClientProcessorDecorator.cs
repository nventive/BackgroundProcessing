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
    /// <see cref="IBackgroundProcessor"/> decorator that adds Application Insights support.
    /// </summary>
    public class TelemetryClientProcessorDecorator : IBackgroundProcessor
    {
        /// <summary>
        /// The name of a successful dispatched event.
        /// </summary>
        public const string BackgroundCommandProcessedEventName = "BackgroundCommandProcessed";

        /// <summary>
        /// The name of the metric that aggregates dispatch time.
        /// </summary>
        public const string BackgroundCommandProcessTimeMetricName = "BackgroundCommandProcessTime";

        /// <summary>
        /// The name of the metric that aggregates latency time.
        /// </summary>
        public const string BackgroundCommandLatencyTimeMetricName = "BackgroundCommandLatencyTime";

        private readonly IBackgroundProcessor _wrappedProcessor;
        private readonly IOptions<TelemetryClientDecoratorOptions> _options;
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryClientProcessorDecorator"/> class.
        /// </summary>
        /// <param name="wrappedProcessor">The inner <see cref="IBackgroundProcessor"/>.</param>
        /// <param name="options">The <see cref="TelemetryClientDecoratorOptions"/>.</param>
        /// <param name="telemetryClient">The <see cref="TelemetryClient"/>.</param>
        public TelemetryClientProcessorDecorator(
            IBackgroundProcessor wrappedProcessor,
            IOptions<TelemetryClientDecoratorOptions> options,
            TelemetryClient telemetryClient)
        {
            _wrappedProcessor = wrappedProcessor ?? throw new ArgumentNullException(nameof(wrappedProcessor));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        /// <inheritdoc />
        public async Task ProcessAsync(IBackgroundCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            try
            {
                var now = DateTimeOffset.UtcNow;
                var stopwatch = Stopwatch.StartNew();
                await _wrappedProcessor.ProcessAsync(command, cancellationToken);
                stopwatch.Stop();
                var eventTelemetry = new EventTelemetry(BackgroundCommandProcessedEventName);
                eventTelemetry.Properties.Add(nameof(command.Id), command.Id);
                eventTelemetry.Properties.Add(nameof(command.Timestamp), command.Timestamp.ToString("O", CultureInfo.InvariantCulture));
                eventTelemetry.Metrics.Add(BackgroundCommandProcessTimeMetricName, stopwatch.ElapsedMilliseconds);
                eventTelemetry.Metrics.Add(BackgroundCommandLatencyTimeMetricName, now.Subtract(command.Timestamp).TotalMilliseconds);
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
