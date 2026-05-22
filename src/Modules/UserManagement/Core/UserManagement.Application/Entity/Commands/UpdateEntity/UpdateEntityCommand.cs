using MediatR;
using Contracts.Common;


namespace UserManagement.Application.Entity.Commands.UpdateEntity
{
    public class UpdateEntityCommand : IRequest<int>, IRequirePermission
    {
    public int Id { get; set; }
    public string? EntityName { get; set; }
    public string? EntityDescription { get; set; }
    public string? Address { get; set; }
    public string? Phone  { get; set; }
    public string? Email { get; set; }
    public byte IsActive { get; set; } 
       
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
