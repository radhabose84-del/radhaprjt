using System.Text.Json;
using UserManagement.Application.Common.Interfaces;
using MassTransit;
using MongoDB.Driver;
using Serilog;
using UserManagement.Infrastructure.Persistence;

namespace UserManagement.Infrastructure.Services
{
    public sealed class EventPublisher : IEventPublisher
    {
        private const string OutboxCollectionName = "OutboxMessages";

        private readonly IMongoCollection<OutboxMessage> _outboxCollection;
        private readonly IPublishEndpoint _publishEndpoint;

        public EventPublisher(IMongoDatabase mongoDatabase, IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            if (mongoDatabase is null) throw new ArgumentNullException(nameof(mongoDatabase));

            _outboxCollection = mongoDatabase.GetCollection<OutboxMessage>(OutboxCollectionName);
        }

        public async Task SaveEventAsync<T>(T @event) where T : class
        {
            if (@event is null) throw new ArgumentNullException(nameof(@event));

            var message = new OutboxMessage
            {
                EventType = @event.GetType().AssemblyQualifiedName!,
                EventData = JsonSerializer.Serialize(@event),
                Processed = false,
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0
            };

            await _outboxCollection.InsertOneAsync(message);
        }

        public async Task PublishPendingEventsAsync()
        {
            var pendingMessages = await _outboxCollection
                .Find(x => !x.Processed)
                .ToListAsync();

            foreach (var message in pendingMessages)
            {
                try
                {
                    var eventType = Type.GetType(message.EventType, throwOnError: false);
                    if (eventType == null)
                    {
                        Log.Warning("Unknown event type: {EventType}", message.EventType);
                        continue;
                    }

                    var @event = JsonSerializer.Deserialize(message.EventData, eventType);
                    if (@event == null)
                    {
                        Log.Warning("Deserialization failed for event type: {EventType}", message.EventType);
                        continue;
                    }

                    await _publishEndpoint.Publish(@event, eventType);

                    message.Processed = true;
                    message.LastPublishedAt = DateTime.UtcNow;
                    message.LastError = null;
                }
                catch (Exception ex)
                {
                    message.RetryCount += 1;
                    message.LastError = ex.ToString();

                    Log.Error(ex, "Error publishing event of type: {EventType}", message.EventType);
                }

                await _outboxCollection.ReplaceOneAsync(x => x.Id == message.Id, message);
            }
        }
    }
}
