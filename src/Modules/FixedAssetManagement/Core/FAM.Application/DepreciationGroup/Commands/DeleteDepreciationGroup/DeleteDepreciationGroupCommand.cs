using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using MediatR;
using Contracts.Common;

namespace FAM.Application.DepreciationGroup.Commands.DeleteDepreciationGroup
{
    public class DeleteDepreciationGroupCommand :  IRequest<DepreciationGroupDTO>, IRequirePermission
    {
          public int Id { get; set; }         
          public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
