using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetGstrSectionMasterAutoComplete
{
    public class GetGstrSectionMasterAutoCompleteQueryHandler : IRequestHandler<GetGstrSectionMasterAutoCompleteQuery, ApiResponseDTO<IReadOnlyList<GstrSectionMasterLookupDto>>>
    {
        private readonly IGstrSectionQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetGstrSectionMasterAutoCompleteQueryHandler(IGstrSectionQueryRepository queryRepository, IIPAddressService ipAddressService, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<IReadOnlyList<GstrSectionMasterLookupDto>>> Handle(GetGstrSectionMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var result = await _queryRepository.SectionAutocompleteAsync(request.Term ?? string.Empty, request.ReportTypeId, companyId, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetGstrSectionMasterAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "GSTR section autocomplete was fetched.",
                module: "GstrSectionMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<IReadOnlyList<GstrSectionMasterLookupDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = result
            };
        }
    }
}
