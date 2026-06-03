using UserManagement.Application.Units.Queries.GetUnits;
using MediatR;
using Contracts.Common;


namespace UserManagement.Application.Units.Commands.CreateUnit
{
    public class CreateUnitCommand : IRequest<int>, IRequirePermission
    {
    public string? UnitName { get; set; }
    public string? ShortName { get; set; }
    public int CompanyId { get; set; }
    public int DivisionId { get; set; }
    public string? UnitHeadName { get; set; }
    public string? CINNO { get; set; }
    public string? OldUnitId { get; set; }
    public bool IsMaintenanceStopStart { get; set; }
    public int? SpindlesCapacity { get; set; }
    public int UnitTypeId { get; set; }
    public int? BankAccountId { get; set; }
    // Default to empty objects so an omitted/empty body fails field validation (400) rather
    // than NRE-ing in the validator's nested rules (which would be a 500).
    public UnitAddressDto UnitAddressDto { get; set; } = new();
    public UnitContactsDto UnitContactsDto { get; set; } = new();
    public PermissionType RequiredPermission => PermissionType.CanAdd;
    }



}
