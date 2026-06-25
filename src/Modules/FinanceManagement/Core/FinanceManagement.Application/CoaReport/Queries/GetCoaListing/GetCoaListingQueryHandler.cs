using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaReport;
using FinanceManagement.Application.CoaReport.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CoaReport.Queries.GetCoaListing
{
    public class GetCoaListingQueryHandler : IRequestHandler<GetCoaListingQuery, ApiResponseDTO<List<CoaListingItemDto>>>
    {
        private readonly ICoaReportQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetCoaListingQueryHandler(
            ICoaReportQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<CoaListingItemDto>>> Handle(GetCoaListingQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var rows = await _queryRepository.GetCoaListingAsync(
                companyId, request.AccountTypeId, request.AccountGroupId, request.ActiveOnly, request.SearchTerm, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetCoaListingQuery",
                actionCode: "Get",
                actionName: rows.Count.ToString(),
                details: "COA listing report was fetched.",
                module: "CoaReport"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<CoaListingItemDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = rows,
                TotalCount = rows.Count
            };
        }
    }
}
