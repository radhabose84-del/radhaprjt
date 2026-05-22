using UserManagement.Application.Units.Queries.GetUnits;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Units.Commands.UpdateUnit
{
    public class UpdateUnitCommand : IRequest<int>, IRequirePermission
    {    
    //public int UnitId  { get; set; }
    public UpdateUnitsDto? UpdateUnitDto { get; set; }  
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
