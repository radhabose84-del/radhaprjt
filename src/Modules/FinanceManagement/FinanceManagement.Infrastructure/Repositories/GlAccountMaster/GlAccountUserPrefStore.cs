using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace FinanceManagement.Infrastructure.Repositories.GlAccountMaster
{
    // Per-user favourites + recently-used for the account type-ahead (US-GL02-07), stored in MongoDB
    // (no SQL table/migration). Keyed by (UserId, CompanyId, AccountId); upserts keep one row per key.
    internal sealed class GlAccountUserPrefStore : IGlAccountUserPrefStore
    {
        private readonly IMongoCollection<GlAccountFavouriteDoc> _favourites;
        private readonly IMongoCollection<GlAccountRecentUseDoc> _recent;

        public GlAccountUserPrefStore(IMongoDbContext mongo)
        {
            _favourites = mongo.GetCollection<GlAccountFavouriteDoc>("GlAccountFavourite");
            _recent = mongo.GetCollection<GlAccountRecentUseDoc>("GlAccountRecentUse");
        }

        public async Task<IReadOnlyList<int>> GetFavouriteAccountIdsAsync(int userId, int companyId, CancellationToken ct = default)
        {
            var filter = Builders<GlAccountFavouriteDoc>.Filter.Eq(d => d.UserId, userId)
                       & Builders<GlAccountFavouriteDoc>.Filter.Eq(d => d.CompanyId, companyId);
            var docs = await _favourites.Find(filter).ToListAsync(ct);
            return docs.Select(d => d.AccountId).ToList();
        }

        public Task AddFavouriteAsync(int userId, int companyId, int accountId, CancellationToken ct = default)
        {
            var update = Builders<GlAccountFavouriteDoc>.Update
                .SetOnInsert(d => d.UserId, userId)
                .SetOnInsert(d => d.CompanyId, companyId)
                .SetOnInsert(d => d.AccountId, accountId)
                .SetOnInsert(d => d.CreatedDate, DateTime.UtcNow);
            return _favourites.UpdateOneAsync(FavFilter(userId, companyId, accountId), update, new UpdateOptions { IsUpsert = true }, ct);
        }

        public Task RemoveFavouriteAsync(int userId, int companyId, int accountId, CancellationToken ct = default)
            => _favourites.DeleteOneAsync(FavFilter(userId, companyId, accountId), ct);

        public async Task<IReadOnlyList<GlAccountRecentUseItem>> GetRecentAsync(int userId, int companyId, int take, CancellationToken ct = default)
        {
            var filter = Builders<GlAccountRecentUseDoc>.Filter.Eq(d => d.UserId, userId)
                       & Builders<GlAccountRecentUseDoc>.Filter.Eq(d => d.CompanyId, companyId);
            var docs = await _recent.Find(filter)
                .SortByDescending(d => d.LastUsedDate)
                .Limit(take > 0 ? take : 10)
                .ToListAsync(ct);
            return docs
                .Select(d => new GlAccountRecentUseItem(
                    d.AccountId,
                    new DateTimeOffset(DateTime.SpecifyKind(d.LastUsedDate, DateTimeKind.Utc)),
                    d.UseCount))
                .ToList();
        }

        public Task RecordRecentAsync(int userId, int companyId, int accountId, CancellationToken ct = default)
        {
            var update = Builders<GlAccountRecentUseDoc>.Update
                .Set(d => d.LastUsedDate, DateTime.UtcNow)
                .Inc(d => d.UseCount, 1)                       // 0→1 on insert, +1 on each subsequent select
                .SetOnInsert(d => d.UserId, userId)
                .SetOnInsert(d => d.CompanyId, companyId)
                .SetOnInsert(d => d.AccountId, accountId);
            return _recent.UpdateOneAsync(RecentFilter(userId, companyId, accountId), update, new UpdateOptions { IsUpsert = true }, ct);
        }

        private static FilterDefinition<GlAccountFavouriteDoc> FavFilter(int userId, int companyId, int accountId) =>
            Builders<GlAccountFavouriteDoc>.Filter.Eq(d => d.UserId, userId)
          & Builders<GlAccountFavouriteDoc>.Filter.Eq(d => d.CompanyId, companyId)
          & Builders<GlAccountFavouriteDoc>.Filter.Eq(d => d.AccountId, accountId);

        private static FilterDefinition<GlAccountRecentUseDoc> RecentFilter(int userId, int companyId, int accountId) =>
            Builders<GlAccountRecentUseDoc>.Filter.Eq(d => d.UserId, userId)
          & Builders<GlAccountRecentUseDoc>.Filter.Eq(d => d.CompanyId, companyId)
          & Builders<GlAccountRecentUseDoc>.Filter.Eq(d => d.AccountId, accountId);

        internal sealed class GlAccountFavouriteDoc
        {
            [BsonId] public ObjectId Id { get; set; }
            public int UserId { get; set; }
            public int CompanyId { get; set; }
            public int AccountId { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        internal sealed class GlAccountRecentUseDoc
        {
            [BsonId] public ObjectId Id { get; set; }
            public int UserId { get; set; }
            public int CompanyId { get; set; }
            public int AccountId { get; set; }
            public DateTime LastUsedDate { get; set; }
            public int UseCount { get; set; }
        }
    }
}
