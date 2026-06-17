using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetAllGstrSectionAccountLinkage
{
    public class GetAllGstrSectionAccountLinkageQueryHandler : IRequestHandler<GetAllGstrSectionAccountLinkageQuery, ApiResponseDTO<List<GstrSectionAccountLinkageDto>>>
    {
        private readonly IGstrSectionQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetAllGstrSectionAccountLinkageQueryHandler(IGstrSectionQueryRepository queryRepository, IIPAddressService ipAddressService, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<GstrSectionAccountLinkageDto>>> Handle(GetAllGstrSectionAccountLinkageQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetAllLinkagesAsync(request.PageNumber, request.PageSize, request.SearchTerm, companyId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllGstrSectionAccountLinkageQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "GSTR section-account mapping details were fetched.",
                module: "GstrSectionAccountLinkage"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<GstrSectionAccountLinkageDto>>
            {
                IsSuccess = true,
                Message = "GSTR section-account mapping list retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
