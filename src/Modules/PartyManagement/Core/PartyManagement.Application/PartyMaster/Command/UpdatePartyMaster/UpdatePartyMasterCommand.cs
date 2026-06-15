using MediatR;
using Contracts.Common;

namespace PartyManagement.Application.PartyMaster.Command.UpdatePartyMaster
{
    public class UpdatePartyMasterCommand  : IRequest<bool>, IRequirePermission
    {
         public UpdatePartyMasterDto? UpdatePartyMaster { get; set; }
         public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
