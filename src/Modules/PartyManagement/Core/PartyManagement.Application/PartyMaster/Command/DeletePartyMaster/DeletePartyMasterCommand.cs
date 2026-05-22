using MediatR;
using Contracts.Common;

namespace PartyManagement.Application.PartyMaster.Command.DeletePartyMaster
{
    public class DeletePartyMasterCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }   
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
