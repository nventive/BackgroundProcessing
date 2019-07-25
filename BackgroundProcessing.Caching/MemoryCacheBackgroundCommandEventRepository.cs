using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core.Events;
using Microsoft.Extensions.Caching.Memory;

namespace BackgroundProcessing.Caching
{
    /// <summary>
    /// <see cref="IBackgroundCommandEventRepository"/> implementation that uses <see cref="IMemoryCache"/>.
    /// </summary>
    /// <remarks>
    /// Be mindful that there is no concurrency measures implemented to not throttle throughput of execution.
    /// As such, events may be missed or overriden.
    /// </remarks>
    public class MemoryCacheBackgroundCommandEventRepository : IBackgroundCommandEventRepository
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _expiration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheBackgroundCommandEventRepository"/> class.
        /// </summary>
        /// <param name="cache">The <see cref="IMemoryCache"/>.</param>
        /// <param name="expiration">The duration of items in the cache before expiring.</param>
        public MemoryCacheBackgroundCommandEventRepository(
            IMemoryCache cache,
            TimeSpan expiration)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _expiration = expiration;
        }

        /// <inheritdoc />
        public async Task Add(BackgroundCommandEvent commandEvent, CancellationToken cancellationToken = default)
        {
            if (commandEvent is null)
            {
                throw new ArgumentNullException(nameof(commandEvent));
            }

            var allCommandEvents = _cache.Get<IList<BackgroundCommandEvent>>(commandEvent.Command.Id) ?? new List<BackgroundCommandEvent>();
            allCommandEvents.Add(commandEvent);
            _cache.Set(commandEvent.Command.Id, allCommandEvents, _expiration);
        }

        /// <inheritdoc />
        public async Task<BackgroundCommandEvent> GetLatestEventForCommandId(string commandId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(commandId))
            {
                throw new ArgumentNullException(nameof(commandId));
            }

            var allEvents = _cache.Get<IList<BackgroundCommandEvent>>(commandId);
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

            var allEvents = _cache.Get<IList<BackgroundCommandEvent>>(commandId);
            if (allEvents == null)
            {
                return Enumerable.Empty<BackgroundCommandEvent>();
            }

            return allEvents.OrderByDescending(x => x.Timestamp).ThenByDescending(x => x.Status).ToList();
        }
    }
}
