using MongoDB.Driver;
using MediatR;
using Core.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.AuditLog.Queries.GetAuditLog
{
    public class GetAuditLogQueryHandler : IRequestHandler<GetAuditLogQuery, List<AuditLogDto>>
    {
        private readonly IMongoCollection<AuditLogs> _auditLogCollection;

        public GetAuditLogQueryHandler(IMongoDatabase mongoDatabase)
        {
            // Initialize the collection for AuditLogs in MongoDB
            _auditLogCollection = mongoDatabase.GetCollection<AuditLogs>("AuditLogs");
        }

        public async Task<List<AuditLogDto>> Handle(GetAuditLogQuery request, CancellationToken cancellationToken)
        {
            
            //var filter = Builders<AuditLogs>.Filter.Eq(log => log.IsActive, 1); // Active logs filter            
            //var auditLogsCursor = await _auditLogCollection.FindAsync(filter, cancellationToken: cancellationToken);

            var auditLogsCursor = await _auditLogCollection.FindAsync(_ => true, cancellationToken: cancellationToken);

            var auditLogs = await auditLogsCursor.ToListAsync(cancellationToken);

            // Map the MongoDB AuditLogs entities to AuditLogDto (if necessary)
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

            // Return the result
            return auditLogDto;
        }
    }
}
