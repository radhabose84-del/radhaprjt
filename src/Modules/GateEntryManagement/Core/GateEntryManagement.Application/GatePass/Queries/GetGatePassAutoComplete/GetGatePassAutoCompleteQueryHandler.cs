using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Application.GatePass.Dto;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.GatePass.Queries.GetGatePassAutoComplete
{
    public class GetGatePassAutoCompleteQueryHandler : IRequestHandler<GetGatePassAutoCompleteQuery, IReadOnlyList<GatePassAutoCompleteDto>>
    {
        private readonly IGatePassQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetGatePassAutoCompleteQueryHandler(IGatePassQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<GatePassAutoCompleteDto>> Handle(GetGatePassAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetGatePassAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "GatePass details was fetched.",
                module: "GatePass"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
