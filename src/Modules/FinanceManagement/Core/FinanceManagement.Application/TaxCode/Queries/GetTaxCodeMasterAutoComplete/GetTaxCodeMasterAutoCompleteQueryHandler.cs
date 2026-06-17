using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxCodeMasterAutoComplete
{
    public class GetTaxCodeMasterAutoCompleteQueryHandler : IRequestHandler<GetTaxCodeMasterAutoCompleteQuery, IReadOnlyList<TaxCodeMasterLookupDto>>
    {
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetTaxCodeMasterAutoCompleteQueryHandler(ITaxCodeQueryRepository queryRepository, IIPAddressService ipAddressService, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<TaxCodeMasterLookupDto>> Handle(GetTaxCodeMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var result = await _queryRepository.TaxCodeAutocompleteAsync(request.Term, companyId, request.TaxType, cancellationToken);
            var dtos = _mapper.Map<List<TaxCodeMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetTaxCodeMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "TaxCodeMaster details was fetched.",
                module: "TaxCodeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
