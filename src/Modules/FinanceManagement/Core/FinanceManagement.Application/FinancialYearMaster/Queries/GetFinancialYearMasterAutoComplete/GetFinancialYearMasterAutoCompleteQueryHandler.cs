using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialYearMasterAutoComplete
{
    public class GetFinancialYearMasterAutoCompleteQueryHandler : IRequestHandler<GetFinancialYearMasterAutoCompleteQuery, IReadOnlyList<FinancialYearMasterLookupDto>>
    {
        private readonly IFinancialYearMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetFinancialYearMasterAutoCompleteQueryHandler(
            IFinancialYearMasterQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<FinancialYearMasterLookupDto>> Handle(GetFinancialYearMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var result = await _queryRepository.AutocompleteAsync(request.Term, companyId, cancellationToken);
            var dtos = _mapper.Map<List<FinancialYearMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetFinancialYearMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "FinancialYearMaster details was fetched.",
                module: "FinancialYearMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
