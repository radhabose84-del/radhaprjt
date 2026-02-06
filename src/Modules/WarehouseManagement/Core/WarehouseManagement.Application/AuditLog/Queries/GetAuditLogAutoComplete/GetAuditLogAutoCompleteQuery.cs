using WarehouseManagement.Application.AuditLog.Queries.GetAuditLog;
using WarehouseManagement.Application.Common.HttpResponse;
using MediatR;

namespace WarehouseManagement.Application.AuditLog.Queries
{
    public class GetAuditLogBySearchPatternQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
        public string? SearchPattern { get; set; } 
    }
}
