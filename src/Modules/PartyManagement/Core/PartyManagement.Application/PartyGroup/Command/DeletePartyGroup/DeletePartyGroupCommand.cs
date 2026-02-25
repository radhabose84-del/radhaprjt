using MediatR;

namespace PartyManagement.Application.PartyGroup.Command.DeletePartyGroup
{
    public class DeletePartyGroupCommand : IRequest<bool>
    {
      public int Id { get; set; }   
    }
}