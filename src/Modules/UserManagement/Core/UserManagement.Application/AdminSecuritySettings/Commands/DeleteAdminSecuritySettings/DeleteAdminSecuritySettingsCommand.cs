using MediatR;
using Contracts.Common;


namespace UserManagement.Application.AdminSecuritySettings.Commands.DeleteAdminSecuritySettings
{
    public class DeleteAdminSecuritySettingsCommand : IRequest<int>, IRequirePermission
    {

         public int Id { get; set; }      
         public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
