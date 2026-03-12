using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanceManagement.Infrastructure.Persistence
{
    public class OutboxMessage
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string? EventType { get; set; }
        public string? EventData { get; set; }
        public bool Processed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastPublishedAt { get; set; }
        public int RetryCount { get; set; }
        public string? LastError { get; set; }
    }
}
