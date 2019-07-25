using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core.Events;
using Microsoft.Extensions.Caching.Distributed;

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
        private readonly IBackgroundCommandEventsSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheBackgroundCommandEventRepository"/> class.
        /// </summary>
        /// <param name="cache">The <see cref="IDistributedCache"/>.</param>
        /// <param name="expiration">The duration of items in the cache before expiring.</param>
        /// <param name="serializer">The <see cref="IBackgroundCommandEventsSerializer"/>.</param>
        public DistributedCacheBackgroundCommandEventRepository(
            IDistributedCache cache,
            TimeSpan expiration,
            IBackgroundCommandEventsSerializer serializer)
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

            return allEvents.OrderByDescending(x => x.Timestamp).FirstOrDefault();
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

            return allEvents.OrderByDescending(x => x.Timestamp).ToList();
        }

        private async Task<IList<BackgroundCommandEvent>> GetFromCache(string commandId, CancellationToken cancellationToken)
        {
            var stringResult = await _cache.GetStringAsync(commandId, cancellationToken);
            if (string.IsNullOrEmpty(stringResult))
            {
                return null;
            }

            return await _serializer.DeserializeAsync(stringResult, cancellationToken);
        }

        private async Task Set(IList<BackgroundCommandEvent> events, CancellationToken cancellationToken)
        {
            var stringValue = await _serializer.SerializeAsync(events, cancellationToken);
            await _cache.SetStringAsync(events.First().Command.Id, stringValue, _distributedCacheEntryOptions, cancellationToken);
        }
    }
}
