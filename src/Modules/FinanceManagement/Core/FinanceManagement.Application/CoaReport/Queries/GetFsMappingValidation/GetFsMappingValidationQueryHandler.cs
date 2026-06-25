using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaReport;
using FinanceManagement.Application.CoaReport.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CoaReport.Queries.GetFsMappingValidation
{
    public class GetFsMappingValidationQueryHandler : IRequestHandler<GetFsMappingValidationQuery, ApiResponseDTO<FsMappingValidationDto>>
    {
        private readonly ICoaReportQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetFsMappingValidationQueryHandler(
            ICoaReportQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<FsMappingValidationDto>> Handle(GetFsMappingValidationQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var result = await _queryRepository.GetFsMappingValidationAsync(companyId, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetFsMappingValidationQuery",
                actionCode: "Get",
                actionName: result.UnmappedCount.ToString(),
                details: $"FS-mapping validation: {result.UnmappedCount} unmapped leaf group(s) of {result.TotalLeafGroups}.",
                module: "CoaReport"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<FsMappingValidationDto>
            {
                IsSuccess = true,
                Message = result.IsClean ? "All leaf groups are mapped." : $"{result.UnmappedCount} unmapped leaf group(s) remain.",
                Data = result
            };
        }
    }
}
