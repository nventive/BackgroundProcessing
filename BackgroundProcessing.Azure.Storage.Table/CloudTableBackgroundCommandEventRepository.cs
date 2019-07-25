using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using BackgroundProcessing.Core.Events;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace BackgroundProcessing.Azure.Storage.Table
{
    /// <summary>
    /// <see cref="IBackgroundCommandEventRepository"/> that uses <see cref="CloudTable"/>.
    /// </summary>
    public class CloudTableBackgroundCommandEventRepository : IBackgroundCommandEventRepository
    {
        private readonly CloudTable _cloudTable;
        private readonly IBackgroundCommandSerializer _commandSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudTableBackgroundCommandEventRepository"/> class.
        /// </summary>
        /// <param name="cloudTable">The <see cref="CloudTable"/>.</param>
        /// <param name="commandSerializer">The <see cref="IBackgroundCommandSerializer"/>.</param>
        public CloudTableBackgroundCommandEventRepository(
            CloudTable cloudTable,
            IBackgroundCommandSerializer commandSerializer)
        {
            _cloudTable = cloudTable ?? throw new ArgumentNullException(nameof(cloudTable));
            _commandSerializer = commandSerializer;
        }

        /// <inheritdoc />
        public async Task Add(BackgroundCommandEvent commandEvent, CancellationToken cancellationToken = default)
        {
            if (commandEvent is null)
            {
                throw new ArgumentNullException(nameof(commandEvent));
            }

            var operation = TableOperation.Insert(new EventTableEntity(commandEvent, _commandSerializer));
            await _cloudTable.ExecuteAsync(operation, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<BackgroundCommandEvent>> GetAllEventsForCommandId(string commandId, CancellationToken cancellationToken = default)
        {
            var query = new TableQuery<EventTableEntity>()
                .Where(TableQuery.GenerateFilterCondition(nameof(EventTableEntity.PartitionKey), QueryComparisons.Equal, commandId))
                .OrderByDesc(nameof(EventTableEntity.EventTimestamp));
            return _cloudTable.ExecuteQuery(query).Select(x => x.ToBackgroundCommandEvent(_commandSerializer));
        }

        /// <inheritdoc />
        public async Task<BackgroundCommandEvent> GetLatestEventForCommandId(string commandId, CancellationToken cancellationToken = default)
        {
            var query = new TableQuery<EventTableEntity>()
                .Where(TableQuery.GenerateFilterCondition(nameof(EventTableEntity.PartitionKey), QueryComparisons.Equal, commandId))
                .OrderByDesc(nameof(EventTableEntity.EventTimestamp))
                .Take(1);

            return _cloudTable.ExecuteQuery(query).Select(x => x.ToBackgroundCommandEvent(_commandSerializer)).FirstOrDefault();
        }

        private class EventTableEntity : TableEntity
        {
            public EventTableEntity()
            {
            }

            [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Let events serialize in any case.")]
            public EventTableEntity(BackgroundCommandEvent commandEvent, IBackgroundCommandSerializer serializer)
            {
                PartitionKey = commandEvent.Command.Id;
                RowKey = BackgroundCommandIdGenerator.Generate();
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
