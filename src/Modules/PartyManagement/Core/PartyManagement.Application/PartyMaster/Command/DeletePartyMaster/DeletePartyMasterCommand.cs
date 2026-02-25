using MediatR;

namespace PartyManagement.Application.PartyMaster.Command.DeletePartyMaster
{
    public class DeletePartyMasterCommand : IRequest<bool>
    {
        public int Id { get; set; }   
    }
}