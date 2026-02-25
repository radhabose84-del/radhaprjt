using MediatR;

namespace PartyManagement.Application.PartyMaster.Command.UpdatePartyMaster
{
    public class UpdatePartyMasterCommand  : IRequest<bool>
    {
         public UpdatePartyMasterDto? UpdatePartyMaster { get; set; }
    }
}