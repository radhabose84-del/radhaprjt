using GateEntryManagement.Domain.Entities;
using MediatR;
using MongoDB.Driver;

namespace GateEntryManagement.Application.AuditLog.Queries.GetAuditLog
{
    public class GetAuditLogQueryHandler : IRequestHandler<GetAuditLogQuery, List<AuditLogDto>>
    {
        private readonly IMongoCollection<AuditLogs> _auditLogCollection;

        public GetAuditLogQueryHandler(IMongoDatabase mongoDatabase)
        {
            _auditLogCollection = mongoDatabase.GetCollection<AuditLogs>("AuditLogs");
        }

        public async Task<List<AuditLogDto>> Handle(GetAuditLogQuery request, CancellationToken cancellationToken)
        {
            var auditLogs = await _auditLogCollection.Find(_ => true).ToListAsync(cancellationToken);

            return auditLogs.Select(log => new AuditLogDto
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
        }
    }
}
