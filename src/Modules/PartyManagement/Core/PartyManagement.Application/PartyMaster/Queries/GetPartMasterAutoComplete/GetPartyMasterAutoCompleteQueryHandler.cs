#nullable disable
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartMasterAutoComplete
{
    public class GetPartyMasterAutoCompleteQueryHandler : IRequestHandler<GetPartyMasterAutoCompleteQuery, List<GetPartyMasterAutoCompleteDto>>
    {
        private readonly IPartyMasterQueryRepository _ipartyMasterQueryRepository;
        private readonly IMediator _mediator;
        public GetPartyMasterAutoCompleteQueryHandler(IPartyMasterQueryRepository ipartyMasterQueryRepository, IMediator mediator)
        {
            _ipartyMasterQueryRepository = ipartyMasterQueryRepository;
            _mediator = mediator;
        }
        public async Task<List<GetPartyMasterAutoCompleteDto>> Handle(GetPartyMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var partymaster = await _ipartyMasterQueryRepository.GetPartyMasterAutoComplete(request.PartyTypeIds, request.SearchPattern ?? string.Empty, request.AgentId);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetPartyMasterAutoCompleteQuery",
                actionName: partymaster.Count.ToString(),
                details: $"PartyMaster details was fetched.",
                module:"PartyMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return partymaster;
        }
    }
}