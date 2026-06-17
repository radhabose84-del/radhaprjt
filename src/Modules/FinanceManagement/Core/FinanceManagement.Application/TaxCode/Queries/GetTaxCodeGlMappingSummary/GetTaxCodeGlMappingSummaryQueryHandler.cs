using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxCodeGlMappingSummary
{
    public class GetTaxCodeGlMappingSummaryQueryHandler : IRequestHandler<GetTaxCodeGlMappingSummaryQuery, ApiResponseDTO<List<TaxCodeGlMappingSummaryDto>>>
    {
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetTaxCodeGlMappingSummaryQueryHandler(ITaxCodeQueryRepository queryRepository, IIPAddressService ipAddressService, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<TaxCodeGlMappingSummaryDto>>> Handle(GetTaxCodeGlMappingSummaryQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetTaxCodeGlMappingSummaryAsync(
                request.PageNumber, request.PageSize, request.SearchTerm, companyId, request.TaxType);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetTaxCodeGlMappingSummaryQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Tax code GL-mapping summary was fetched.",
                module: "TaxCodeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<TaxCodeGlMappingSummaryDto>>
            {
                IsSuccess = true,
                Message = "Tax code GL-mapping summary retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
