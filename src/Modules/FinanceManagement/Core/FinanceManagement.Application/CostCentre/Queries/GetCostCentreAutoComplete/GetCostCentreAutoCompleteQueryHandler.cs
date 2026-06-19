using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Application.CostCentre.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CostCentre.Queries.GetCostCentreAutoComplete
{
    public class GetCostCentreAutoCompleteQueryHandler : IRequestHandler<GetCostCentreAutoCompleteQuery, IReadOnlyList<CostCentreLookupDto>>
    {
        private readonly ICostCentreQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCostCentreAutoCompleteQueryHandler(
            ICostCentreQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<CostCentreLookupDto>> Handle(GetCostCentreAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var unitId = _ipAddressService.GetUnitId()
                ?? throw new ExceptionRules("No active unit in session.");

            var result = await _queryRepository.AutocompleteAsync(request.Term, unitId, request.CentreLevelId, cancellationToken);
            var dtos = _mapper.Map<List<CostCentreLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetCostCentreAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "CostCentre details was fetched.",
                module: "CostCentre"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
