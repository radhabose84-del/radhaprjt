using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById
{
    public class GetPartyMasterByIdQuery : IRequest<PartyMasterDto>
    {
        public int PartyId { get; set; }
    }
}