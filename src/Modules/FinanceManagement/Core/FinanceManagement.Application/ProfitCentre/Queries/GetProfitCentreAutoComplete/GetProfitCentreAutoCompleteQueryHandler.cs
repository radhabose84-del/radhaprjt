using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Application.ProfitCentre.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ProfitCentre.Queries.GetProfitCentreAutoComplete
{
    public class GetProfitCentreAutoCompleteQueryHandler : IRequestHandler<GetProfitCentreAutoCompleteQuery, IReadOnlyList<ProfitCentreLookupDto>>
    {
        private readonly IProfitCentreQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetProfitCentreAutoCompleteQueryHandler(
            IProfitCentreQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<ProfitCentreLookupDto>> Handle(GetProfitCentreAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term, request.LevelId, cancellationToken);
            var dtos = _mapper.Map<List<ProfitCentreLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetProfitCentreAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "ProfitCentre details was fetched.",
                module: "ProfitCentre"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
