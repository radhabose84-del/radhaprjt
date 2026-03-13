using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.AuditLog;

namespace ProductionManagement.Application.AuditLog.Queries.GetAuditLog
{
    public class GetAuditLogQueryHandler : IRequestHandler<GetAuditLogQuery, ApiResponseDTO<List<AuditLogDto>>>
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public GetAuditLogQueryHandler(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<ApiResponseDTO<List<AuditLogDto>>> Handle(GetAuditLogQuery request, CancellationToken cancellationToken)
        {
            var logs = await _auditLogRepository.GetAllAsync();

            if (logs is null || logs.Count == 0)
            {
                return new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = false,
                    Message = "No audit logs found."
                };
            }

            var dtos = logs.Select(l => new AuditLogDto
            {
                Id = l.Id.ToString(),
                Action = l.Action,
                Details = l.Details,
                Module = l.Module,
                MachineName = l.MachineName,
                IPAddress = l.IPAddress,
                OS = l.OS,
                Browser = l.Browser,
                CreatedAt = l.CreatedAt,
                CreatedBy = l.CreatedBy,
                CreatedByName = l.CreatedByName
            }).ToList();

            return new ApiResponseDTO<List<AuditLogDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtos
            };
        }
    }
}
