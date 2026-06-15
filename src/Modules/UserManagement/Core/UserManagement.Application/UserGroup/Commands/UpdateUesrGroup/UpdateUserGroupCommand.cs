using MediatR;
using Contracts.Common;

namespace UserManagement.Application.UserGroup.Commands.UpdateUesrGroup
{
    public class UpdateUserGroupCommand : IRequest<bool>, IRequirePermission
       {
              public int Id { get; set; }
              public string? GroupCode { get; set; }
              public string? GroupName { get; set; }
              public byte IsActive { get; set; } 
              public PermissionType RequiredPermission => PermissionType.CanUpdate;
         }
  
}
