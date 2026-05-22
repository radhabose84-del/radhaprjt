using MediatR;
using Contracts.Common;

namespace PartyManagement.Application.PartyMaster.Command.CreatePartyMaster
{
    public class CreatePartyMasterCommand : IRequest<int>, IRequirePermission
    {
        public CreatePartyMasterDto PartyMaster { get; set; } = null!;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
