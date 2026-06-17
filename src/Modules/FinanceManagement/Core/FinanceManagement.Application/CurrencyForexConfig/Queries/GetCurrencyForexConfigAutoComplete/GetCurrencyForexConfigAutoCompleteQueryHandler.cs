using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CurrencyForexConfig.Queries.GetCurrencyForexConfigAutoComplete
{
    public class GetCurrencyForexConfigAutoCompleteQueryHandler : IRequestHandler<GetCurrencyForexConfigAutoCompleteQuery, IReadOnlyList<CurrencyForexConfigLookupDto>>
    {
        private readonly ICurrencyForexConfigQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCurrencyForexConfigAutoCompleteQueryHandler(
            ICurrencyForexConfigQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<CurrencyForexConfigLookupDto>> Handle(GetCurrencyForexConfigAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var result = await _queryRepository.AutocompleteAsync(request.Term, companyId, cancellationToken);
            var dtos = _mapper.Map<List<CurrencyForexConfigLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetCurrencyForexConfigAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "CurrencyForexConfig details was fetched.",
                module: "CurrencyForexConfig"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
