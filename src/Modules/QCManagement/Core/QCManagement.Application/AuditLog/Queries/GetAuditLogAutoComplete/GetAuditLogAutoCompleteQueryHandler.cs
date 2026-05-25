using Contracts.Common;
using QCManagement.Application.AuditLog.Queries.GetAuditLog;
using QCManagement.Domain.Entities;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace QCManagement.Application.AuditLog.Queries.GetAuditLogAutoComplete
{
    public class GetAuditLogAutoCompleteQueryHandler : IRequestHandler<GetAuditLogAutoCompleteQuery, ApiResponseDTO<List<AuditLogDto>>>
    {
        private readonly IMongoCollection<AuditLogs> _auditLogCollection;

        public GetAuditLogAutoCompleteQueryHandler(IMongoDatabase mongoDatabase)
        {
            _auditLogCollection = mongoDatabase.GetCollection<AuditLogs>("AuditLogs");
        }

        public async Task<ApiResponseDTO<List<AuditLogDto>>> Handle(GetAuditLogAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var pattern = request.SearchPattern ?? string.Empty;
            var filter = Builders<AuditLogs>.Filter.Or(
                Builders<AuditLogs>.Filter.Regex("CreatedByName", new BsonRegularExpression(pattern, "i")),
                Builders<AuditLogs>.Filter.Regex("Action", new BsonRegularExpression(pattern, "i")),
                Builders<AuditLogs>.Filter.Regex("Details", new BsonRegularExpression(pattern, "i"))
            );

            var auditLogs = await _auditLogCollection.Find(filter).ToListAsync(cancellationToken);

            if (auditLogs is null || auditLogs.Count == 0)
            {
                return new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = false,
                    Message = "No audit logs found matching the search pattern."
                };
            }

            var auditLogDtos = auditLogs.Select(log => new AuditLogDto
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
                CreatedAt = log.CreatedAt,
                MachineName = log.MachineName
            }).ToList();

            return new ApiResponseDTO<List<AuditLogDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = auditLogDtos
            };
        }
    }
}
