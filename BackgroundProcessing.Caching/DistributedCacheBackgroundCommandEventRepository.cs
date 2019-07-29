using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using BackgroundProcessing.Core.Events;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace BackgroundProcessing.Caching
{
    /// <summary>
    /// <see cref="IBackgroundCommandEventRepository"/> implementation that uses <see cref="IDistributedCache"/>.
    /// </summary>
    /// <remarks>
    /// Be mindful that there is no concurrency measures implemented to not throttle throughput of execution.
    /// As such, events may be missed or overriden.
    /// </remarks>
    public class DistributedCacheBackgroundCommandEventRepository : IBackgroundCommandEventRepository
    {
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _distributedCacheEntryOptions;
        private readonly IBackgroundCommandSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheBackgroundCommandEventRepository"/> class.
        /// </summary>
        /// <param name="cache">The <see cref="IDistributedCache"/>.</param>
        /// <param name="expiration">The duration of items in the cache before expiring.</param>
        /// <param name="serializer">The <see cref="IBackgroundCommandSerializer"/>.</param>
        public DistributedCacheBackgroundCommandEventRepository(
            IDistributedCache cache,
            TimeSpan expiration,
            IBackgroundCommandSerializer serializer)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _distributedCacheEntryOptions = new DistributedCacheEntryOptions { SlidingExpiration = expiration };
        }

        /// <inheritdoc />
        public async Task Add(BackgroundCommandEvent commandEvent, CancellationToken cancellationToken = default)
        {
            if (commandEvent is null)
            {
                throw new ArgumentNullException(nameof(commandEvent));
            }

            var allCommandEvents = (await GetFromCache(commandEvent.Command.Id, cancellationToken)) ?? new List<BackgroundCommandEvent>();
            allCommandEvents.Add(commandEvent);
            await Set(allCommandEvents, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<BackgroundCommandEvent> GetLatestEventForCommandId(string commandId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(commandId))
            {
                throw new ArgumentNullException(nameof(commandId));
            }

            var allEvents = await GetFromCache(commandId, cancellationToken);
            if (allEvents == null)
            {
                return null;
            }

            return allEvents.OrderByDescending(x => x.Timestamp).ThenByDescending(x => x.Status).FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<BackgroundCommandEvent>> GetAllEventsForCommandId(string commandId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(commandId))
            {
                throw new ArgumentNullException(nameof(commandId));
            }

            var allEvents = await GetFromCache(commandId, cancellationToken);
            if (allEvents == null)
            {
                return Enumerable.Empty<BackgroundCommandEvent>();
            }

            return allEvents.OrderByDescending(x => x.Timestamp).ThenByDescending(x => x.Status).ToList();
        }

        private async Task<IList<BackgroundCommandEvent>> GetFromCache(string commandId, CancellationToken cancellationToken)
        {
            var stringResult = await _cache.GetStringAsync(commandId, cancellationToken);
            if (string.IsNullOrEmpty(stringResult))
            {
                return null;
            }

            var eventCacheEntries = JsonConvert.DeserializeObject<List<EventCacheEntry>>(stringResult);
            return eventCacheEntries.Select(x => x.ToBackgroundCommandEvent(_serializer)).ToList();
        }

        private async Task Set(IList<BackgroundCommandEvent> events, CancellationToken cancellationToken)
        {
            var stringValue = JsonConvert.SerializeObject(events.Select(x => new EventCacheEntry(x, _serializer)));
            await _cache.SetStringAsync(events.First().Command.Id, stringValue, _distributedCacheEntryOptions, cancellationToken);
        }

        private class EventCacheEntry
        {
            public EventCacheEntry()
            {
            }

            [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Let events serialize in any case.")]
            public EventCacheEntry(BackgroundCommandEvent commandEvent, IBackgroundCommandSerializer serializer)
            {
                EventStatus = commandEvent.Status.ToString();
                EventTimestamp = commandEvent.Timestamp;
                Command = serializer.SerializeAsync(commandEvent.Command).Result;
                if (commandEvent.Exception != null)
                {
                    try
                    {
                        Exception = JsonConvert.SerializeObject(commandEvent.Exception);
                    }
                    catch (Exception ex)
                    {
                        Exception = ex.ToString();
                    }
                }
            }

            public string EventStatus { get; set; }

            public DateTimeOffset EventTimestamp { get; set; }

            public string Command { get; set; }

            public string Exception { get; set; }

            [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Let events deserialize in any case.")]
            public BackgroundCommandEvent ToBackgroundCommandEvent(IBackgroundCommandSerializer serializer)
            {
                var status = BackgroundCommandEventStatus.Unknown;
                if (Enum.TryParse<BackgroundCommandEventStatus>(EventStatus, out var parsedStatus))
                {
                    status = parsedStatus;
                }

                Exception commandException = null;
                try
                {
                    if (!string.IsNullOrEmpty(Exception))
                    {
                        commandException = JsonConvert.DeserializeObject<Exception>(Exception);
                    }
                }
                catch
                {
                }

                return new BackgroundCommandEvent(serializer.DeserializeAsync(Command).Result, status, EventTimestamp, commandException);
            }
        }
    }
}
