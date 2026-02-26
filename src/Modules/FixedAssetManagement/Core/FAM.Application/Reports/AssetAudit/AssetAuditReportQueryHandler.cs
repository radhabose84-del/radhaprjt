using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IReports;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.Reports.AssetAudit
{
    public class AssetAuditReportQueryHandler : IRequestHandler<AssetAuditReportQuery, ApiResponseDTO<List<AssetAuditReportDto>>>
    {
        private readonly IReportRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public AssetAuditReportQueryHandler(IReportRepository repository, IMapper mapper, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AssetAuditReportDto>>> Handle(AssetAuditReportQuery request, CancellationToken cancellationToken)
        {
            // Fetch AssetAudit report data from repository
            var AssetAuditReports = await _repository.AssetAuditReportAsync(request.AuditCycle);

            // Map to DTOs
            var AssetAuditReportDtos = _mapper.Map<List<AssetAuditReportDto>>(AssetAuditReports);
             // Log audit
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "AssetAuditReportQuery",
                actionCode: "Get",
                actionName: AssetAuditReportDtos.Count.ToString(),
                details: "AssetAudit Report retrieved successfully",
                module: "AssetAudit Reports"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<AssetAuditReportDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = AssetAuditReportDtos ?? new List<AssetAuditReportDto>(),
                TotalCount = AssetAuditReportDtos?.Count ?? 0
            };

        }
    }
}