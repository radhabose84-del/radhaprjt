using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Units.Commands.DeleteUnit
{
    public class DeleteUnitCommand : IRequest<int>, IRequirePermission
    {
            public int UnitId { get; set; }
            public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
 
    }
    
