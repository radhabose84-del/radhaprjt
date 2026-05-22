using UserManagement.Application.State.Queries.GetStates;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.State.Commands.DeleteState
{
    public class DeleteStateCommand :  IRequest<StateDto>, IRequirePermission
       {
                public int Id { get; set; }                
                public PermissionType RequiredPermission => PermissionType.CanDelete;
       }
    
}
