using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Infrastructure.Persistence;
using MassTransit;
using MongoDB.Driver;

namespace BackgroundService.Infrastructure.Services
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
                        
                        continue;
                    }

                    var @event = JsonSerializer.Deserialize(message.EventData, eventType);
                    if (@event == null)
                    {
                        
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
                    
                }

                await _outboxCollection.ReplaceOneAsync(x => x.Id == message.Id, message);
             
            }
        }

    }
}