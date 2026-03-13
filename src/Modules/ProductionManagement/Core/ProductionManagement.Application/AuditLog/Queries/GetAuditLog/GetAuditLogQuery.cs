using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.AuditLog.Queries.GetAuditLog
{
    public class GetAuditLogQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
    }
}
