using LogisticsManagement.Application.Common.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LogisticsManagement.Infrastructure.Services
{
    public class LogQueryService : ILogQueryService
    {
        private readonly IMongoCollection<BsonDocument> _logCollection;

        public LogQueryService(IMongoDbContext mongoDbContext)
        {
            _logCollection = mongoDbContext.GetCollection<BsonDocument>("ApplicationLogs");
        }

        public async Task<string?> GetLatestConnectionFailureAsync()
        {
            var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Gte("UtcTimestamp", fiveMinutesAgo),
                Builders<BsonDocument>.Filter.Regex("RenderedMessage", new BsonRegularExpression("Connection Failed", "i")),
                Builders<BsonDocument>.Filter.Regex("Exception", new BsonRegularExpression("BrokerUnreachableException", "i"))
            );

            var log = await _logCollection
                .Find(filter)
                .SortByDescending(x => x["UtcTimestamp"])
                .Limit(1)
                .FirstOrDefaultAsync();

            return log?["RenderedMessage"]?.AsString ?? log?["Exception"]?.AsString;
        }

        public async Task<string?> GetLatestRollbackErrorAsync(Guid correlationId)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Regex("RenderedMessage", new BsonRegularExpression("Rollback requested", "i")),
                Builders<BsonDocument>.Filter.Eq("Properties.CorrelationId", correlationId.ToString())
            );

            var log = await _logCollection
                .Find(filter)
                .SortByDescending(doc => doc["Timestamp"])
                .FirstOrDefaultAsync();

            return log?["RenderedMessage"]?.AsString;
        }
    }
}
