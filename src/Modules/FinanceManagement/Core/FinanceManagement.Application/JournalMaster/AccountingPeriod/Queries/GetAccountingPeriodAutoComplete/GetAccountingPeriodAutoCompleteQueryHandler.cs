using AutoMapper;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.AccountingPeriod.Queries.GetAccountingPeriodAutoComplete
{
    public class GetAccountingPeriodAutoCompleteQueryHandler : IRequestHandler<GetAccountingPeriodAutoCompleteQuery, IReadOnlyList<AccountingPeriodLookupDto>>
    {
        private readonly IAccountingPeriodQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAccountingPeriodAutoCompleteQueryHandler(IAccountingPeriodQueryRepository queryRepository, IIPAddressService ipAddressService, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<AccountingPeriodLookupDto>> Handle(GetAccountingPeriodAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId();
            var result = await _queryRepository.AutocompleteAsync(request.Term, companyId, request.FinancialYearId, cancellationToken);
            var dtos = _mapper.Map<List<AccountingPeriodLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetAccountingPeriodAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Accounting Period details was fetched.",
                module: "AccountingPeriod"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
