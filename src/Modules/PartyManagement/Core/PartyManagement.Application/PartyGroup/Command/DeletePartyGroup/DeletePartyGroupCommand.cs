using MediatR;
using Contracts.Common;

namespace PartyManagement.Application.PartyGroup.Command.DeletePartyGroup
{
    public class DeletePartyGroupCommand : IRequest<bool>, IRequirePermission
    {
      public int Id { get; set; }   
      public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
