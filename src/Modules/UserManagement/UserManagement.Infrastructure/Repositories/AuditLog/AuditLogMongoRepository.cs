using Core.Application.Common.Interfaces.AuditLog;
using Core.Domain.Entities;
using MongoDB.Driver;
using Microsoft.AspNetCore.Http;
using Core.Application.Common.Interfaces;

namespace UserManagement.Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly IMongoDbContext _mongoDbContext;
        
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogRepository(IMongoDbContext mongoDbContext,IHttpContextAccessor httpContextAccessor)
        {
             _mongoDbContext = mongoDbContext;
            
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<AuditLogs> CreateAsync(AuditLogs auditLog)
        {
            if (auditLog is null) throw new ArgumentNullException(nameof(auditLog));
            await _mongoDbContext.GetCollection<dynamic>("AuditLogs").InsertOneAsync(auditLog);
            return auditLog;
        }
 
        public async Task<List<AuditLogs>> GetAllAsync()
        {
            return await _mongoDbContext.GetCollection<AuditLogs>("AuditLogs").Find(_ => true).ToListAsync();
            
        }

        public async Task<List<AuditLogs>> GetByAuditLogNameAsync(string searchPattern)
        {
            if (string.IsNullOrWhiteSpace(searchPattern))
                throw new ArgumentException("Search pattern cannot be null or empty.", nameof(searchPattern));

            var filter = Builders<AuditLogs>.Filter.Or(
                Builders<AuditLogs>.Filter.Regex("UserName", new MongoDB.Bson.BsonRegularExpression(searchPattern, "i")),
                Builders<AuditLogs>.Filter.Regex("Action", new MongoDB.Bson.BsonRegularExpression(searchPattern, "i")),
                Builders<AuditLogs>.Filter.Regex("Details", new MongoDB.Bson.BsonRegularExpression(searchPattern, "i"))
            );

            return await _mongoDbContext.GetCollection<AuditLogs>("AuditLogs").Find(filter).ToListAsync();
        }
    }
}
 