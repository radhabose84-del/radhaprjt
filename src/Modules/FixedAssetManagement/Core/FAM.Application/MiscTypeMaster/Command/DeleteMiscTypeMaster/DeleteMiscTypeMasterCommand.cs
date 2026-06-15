using MediatR;
using Contracts.Common;

namespace FAM.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommand  : IRequest<bool>, IRequirePermission
    {
          public int Id { get; set; }
          public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
