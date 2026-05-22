using MediatR;
using UserManagement.Application.State.Queries.GetStates;
using Contracts.Common;

namespace UserManagement.Application.State.Commands.CreateState
{
    public class CreateStateCommand : IRequest<StateDto>, IRequirePermission
     {
          public string? StateCode { get; set; }
          public string? StateName { get; set; }
          public int CountryId { get; set; }                
          public PermissionType RequiredPermission => PermissionType.CanAdd;
     }    

}
