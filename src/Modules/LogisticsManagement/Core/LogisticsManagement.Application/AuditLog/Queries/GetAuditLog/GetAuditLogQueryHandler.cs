using MongoDB.Driver;
using MediatR;
using LogisticsManagement.Domain.Entities;

namespace LogisticsManagement.Application.AuditLog.Queries.GetAuditLog
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
            var auditLogsCursor = await _auditLogCollection.FindAsync(_ => true, cancellationToken: cancellationToken);

            var auditLogs = await auditLogsCursor.ToListAsync(cancellationToken);

            var auditLogDto = auditLogs.Select(log => new AuditLogDto
            {
                Id = log.Id.ToString(),
                CreatedBy = log.CreatedBy,
                CreatedByName = log.CreatedByName,
                IPAddress = log.IPAddress,
                MachineName = log.MachineName,
                OS = log.OS,
                Browser = log.Browser,
                Action = log.Action,
                Details = log.Details,
                Module = log.Module,
                CreatedAt = log.CreatedAt
            }).ToList();

            return auditLogDto;
        }
    }
}
