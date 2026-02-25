#nullable disable
using System.Text.Json;
using PurchaseManagement.Application.Common.Interfaces;
using MassTransit;
using MongoDB.Driver;
using PurchaseManagement.Infrastructure.Persistence;
using Serilog;

namespace PurchaseManagement.Infrastructure.Services
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IMongoCollection<OutboxMessage> _outboxCollection;
        private readonly IPublishEndpoint _publishEndpoint;
        public EventPublisher(IMongoCollection<OutboxMessage> outboxCollection, IPublishEndpoint publishEndpoint)
        {
            _outboxCollection = outboxCollection;
            _publishEndpoint = publishEndpoint;
        }
        
          public async Task SaveEventAsync<T>(T @event) where T : class
        {
            var message = new OutboxMessage
            {
                // EventType = @event.GetType().Name,
                // EventData = JsonSerializer.Serialize(@event),
                // Processed = false,
                // CreatedAt = DateTime.UtcNow
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
            var pendingMessages = await _outboxCollection.Find(x => !x.Processed).ToListAsync();

            foreach (var message in pendingMessages)
            {
                try
                {
                    var eventType = Type.GetType(message.EventType);
                    if (eventType == null)
                    {
                        Log.Warning($"Unknown event type: {message.EventType}");
                        continue;
                    }

                    var @event = JsonSerializer.Deserialize(message.EventData, eventType);
                    if (@event == null)
                    {
                        Log.Warning($"Deserialization failed for event type: {message.EventType}");
                        continue;
                    }

                    await _publishEndpoint.Publish(@event, eventType);

                    message.Processed = true;
                    message.LastPublishedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    message.RetryCount += 1;
                    message.LastError = ex.Message;
                    Log.Error(ex, "Error publishing event of type: {EventType}", message.EventType);
                }

                await _outboxCollection.ReplaceOneAsync(x => x.Id == message.Id, message);
                // var pendingMessages = await _outboxCollection.Find(x => !x.Processed).ToListAsync();

                // foreach (var message in pendingMessages)
                // {
                //     try
                //     {
                //         var @event = JsonSerializer.Deserialize<UserCreatedEvent>(message.EventData);
                //         await _publishEndpoint.Publish(@event);

                //         message.Processed = true;
                //         await _outboxCollection.ReplaceOneAsync(x => x.Id == message.Id, message);
                //     }
                //     catch (Exception ex)
                //     {
                //         Log.Information($"Error publishing event: {ex.Message}");
                //     }
            }
        }
    }
}