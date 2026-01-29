using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserManagement.Infrastructure.Persistence
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