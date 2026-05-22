using MediatR;
using Contracts.Common;

namespace UserManagement.Application.State.Commands.UpdateState
{
    public class UpdateStateCommand : IRequest<bool>, IRequirePermission
       {
              public int Id { get; set; }
              public string? StateCode { get; set; }
              public string? StateName { get; set; }
              public int CountryId { get; set; }    
              public byte IsActive { get; set; }      
              public PermissionType RequiredPermission => PermissionType.CanUpdate;
         }
  
}
