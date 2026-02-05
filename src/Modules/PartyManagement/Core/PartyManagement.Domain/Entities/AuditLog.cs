
using System;
using PartyManagement.Domain.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PartyManagement.Domain.Entities
{
    public class AuditLogs  : AuditLogBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public string? Action { get; set; }
        public string? Details { get; set; }
        public string? Module { get; set; }
    }
}