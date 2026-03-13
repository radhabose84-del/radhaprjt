using Contracts.Common;
using MediatR;
using ProductionManagement.Application.AuditLog.Queries.GetAuditLog;
using ProductionManagement.Application.Common.Interfaces.AuditLog;

namespace ProductionManagement.Application.AuditLog.Queries.GetAuditLogAutoComplete
{
    public class GetAuditLogAutoCompleteQueryHandler : IRequestHandler<GetAuditLogAutoCompleteQuery, ApiResponseDTO<List<AuditLogDto>>>
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public GetAuditLogAutoCompleteQueryHandler(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<ApiResponseDTO<List<AuditLogDto>>> Handle(GetAuditLogAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var logs = string.IsNullOrWhiteSpace(request.SearchPattern)
                ? await _auditLogRepository.GetAllAsync()
                : await _auditLogRepository.GetByAuditLogNameAsync(request.SearchPattern);

            if (logs is null || logs.Count == 0)
            {
                return new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = false,
                    Message = "No audit logs found matching the search pattern."
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
