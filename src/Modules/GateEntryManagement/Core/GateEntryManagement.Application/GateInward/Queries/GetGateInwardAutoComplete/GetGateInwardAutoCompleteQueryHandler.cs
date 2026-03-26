using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Dto;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Queries.GetGateInwardAutoComplete
{
    public class GetGateInwardAutoCompleteQueryHandler : IRequestHandler<GetGateInwardAutoCompleteQuery, IReadOnlyList<GateInwardAutoCompleteDto>>
    {
        private readonly IGateInwardQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetGateInwardAutoCompleteQueryHandler(IGateInwardQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<GateInwardAutoCompleteDto>> Handle(GetGateInwardAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll", actionCode: "GetGateInwardAutoCompleteQuery",
                actionName: result.Count.ToString(), details: "GateInward details was fetched.", module: "GateInward");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
