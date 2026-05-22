using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Entity.Commands.CreateEntity
{
    public class CreateEntityCommand :IRequest<int>, IRequirePermission
    {

    public string? EntityName { get; set; }
    public string? EntityDescription { get; set; }
    public string? Address { get; set; }
    public string? Phone  { get; set; }
    public string? Email { get; set; }
   
    public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
