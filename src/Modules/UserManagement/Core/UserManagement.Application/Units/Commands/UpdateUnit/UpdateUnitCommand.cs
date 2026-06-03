using UserManagement.Application.Units.Queries.GetUnits;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Units.Commands.UpdateUnit
{
    public class UpdateUnitCommand : IRequest<int>, IRequirePermission
    {    
    //public int UnitId  { get; set; }
    // Default to an empty object so an omitted/empty body fails field validation (400) instead
    // of NRE-ing in the validator's nested rules (which would be a 500).
    public UpdateUnitsDto UpdateUnitDto { get; set; } = new();
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
