using MediatR;
using Contracts.Common;

namespace UserManagement.Application.City.Commands.DeleteCity
{
    public class DeleteCityCommand :  IRequest<bool>, IRequirePermission
       {
              public int Id { get; set; }                
              public PermissionType RequiredPermission => PermissionType.CanDelete;
       }
    
}
