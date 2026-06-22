using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaFreeze;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace FinanceManagement.Infrastructure.Repositories.CoaFreeze
{
    // App-observed COA freeze violations, in MongoDB (no SQL table). Only UI/API attempts caught by the
    // safety-net are logged; raw-SQL attempts are blocked by the trigger but cannot be logged (rollback).
    internal sealed class CoaFreezeViolationLogStore : ICoaFreezeViolationLog
    {
        private readonly IMongoCollection<CoaFreezeViolationDoc> _collection;

        public CoaFreezeViolationLogStore(IMongoDbContext mongo)
        {
            _collection = mongo.GetCollection<CoaFreezeViolationDoc>("CoaFreezeViolationLog");
        }

        public Task LogAsync(int companyId, int userId, string? operation, CancellationToken ct = default)
            => _collection.InsertOneAsync(new CoaFreezeViolationDoc
            {
                CompanyId = companyId,
                UserId = userId,
                Operation = operation,
                AttemptedAt = DateTime.UtcNow
            }, null, ct);

        public async Task<long> CountSinceAsync(int companyId, DateTimeOffset since, CancellationToken ct = default)
        {
            var filter = Builders<CoaFreezeViolationDoc>.Filter.Eq(d => d.CompanyId, companyId)
                       & Builders<CoaFreezeViolationDoc>.Filter.Gte(d => d.AttemptedAt, since.UtcDateTime);
            return await _collection.CountDocumentsAsync(filter, null, ct);
        }

        internal sealed class CoaFreezeViolationDoc
        {
            [BsonId] public ObjectId Id { get; set; }
            public int CompanyId { get; set; }
            public int UserId { get; set; }
            public string? Operation { get; set; }
            public DateTime AttemptedAt { get; set; }
        }
    }
}
