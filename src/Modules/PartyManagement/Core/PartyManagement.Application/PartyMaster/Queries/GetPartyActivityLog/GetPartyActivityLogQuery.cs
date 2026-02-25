using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartyActivityLog
{
    public class GetPartyActivityLogQuery : IRequest<List<PartyActivityDto>>
    {
        public int PartyId { get; set; }
    }
}