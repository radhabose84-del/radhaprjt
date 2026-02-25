using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartyGroupLoad
{
    public class GetPartyGroupLoadQuery  : IRequest<List<PartyGroupLoadDto>>
    {
         public List<int>? GroupTypeIds { get; set; } // For multi-select filter
    }
}