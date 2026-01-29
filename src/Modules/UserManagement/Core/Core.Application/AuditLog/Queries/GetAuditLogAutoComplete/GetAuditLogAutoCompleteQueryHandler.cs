using MongoDB.Driver;
using Core.Domain.Entities;
using MediatR;
using Core.Application.AuditLog.Queries.GetAuditLog;
using Core.Application.Common.HttpResponse;

namespace Core.Application.AuditLog.Queries.GetAuditLogBySearchPattern
{
    public class GetAuditLogBySearchPatternQueryHandler : IRequestHandler<GetAuditLogBySearchPatternQuery, ApiResponseDTO<List<AuditLogDto>>>
    {
        private readonly IMongoCollection<AuditLogs> _auditLogCollection;

        public GetAuditLogBySearchPatternQueryHandler(IMongoDatabase mongoDatabase)
        {
            _auditLogCollection = mongoDatabase.GetCollection<AuditLogs>("AuditLogs");
        }
        public async Task<ApiResponseDTO<List<AuditLogDto>>> Handle(GetAuditLogBySearchPatternQuery request, CancellationToken cancellationToken)
        {            
            var filter = Builders<AuditLogs>.Filter.Or(
                Builders<AuditLogs>.Filter.Regex("UserName", new MongoDB.Bson.BsonRegularExpression(request.SearchPattern, "i")),
                Builders<AuditLogs>.Filter.Regex("Action", new MongoDB.Bson.BsonRegularExpression(request.SearchPattern, "i")),
                Builders<AuditLogs>.Filter.Regex("Details", new MongoDB.Bson.BsonRegularExpression(request.SearchPattern, "i"))
            );
            var auditLogs = await _auditLogCollection.Find(filter).ToListAsync(cancellationToken);         
            if (auditLogs is null || auditLogs.Count is 0)
            {                    
                return new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = false,
                    Message = "No audit logs found matching the search pattern."
                };
            }
            var auditLogDto = auditLogs.Select(log => new AuditLogDto
            {      
                Id = log.Id.ToString(),          
                CreatedBy = log.CreatedBy,
                CreatedByName = log.CreatedByName,
                IPAddress = log.IPAddress,
                OS = log.OS,
                Browser = log.Browser,
                Action = log.Action,
                Details = log.Details,
                Module = log.Module,
                CreatedAt = log.CreatedAt ,
                MachineName = log.MachineName
            }).ToList();
            return new ApiResponseDTO<List<AuditLogDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = auditLogDto
            };          
        }
    }
}
