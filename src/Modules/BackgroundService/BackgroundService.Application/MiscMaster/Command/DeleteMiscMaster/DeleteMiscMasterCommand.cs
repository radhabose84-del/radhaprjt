using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.MiscMaster.Command.DeleteMiscMaster
{
    public class DeleteMiscMasterCommand  : IRequest<bool>, IRequirePermission
    {
          public int Id { get; set; }
                
          public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
