using MediatR;

namespace PartyManagement.Application.PartyGroup.Queries.GetPartyGroupById
{
    public class GetPartyGroupByIdQuery : IRequest<PartyGroupByIdDto>
    {
        public int Id { get; set; }
    }
}