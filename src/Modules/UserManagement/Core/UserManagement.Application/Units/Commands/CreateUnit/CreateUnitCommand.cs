using UserManagement.Application.Common;
using Contracts.Common;
using UserManagement.Application.Units.Queries.GetUnits;
using MediatR;


namespace UserManagement.Application.Units.Commands.CreateUnit
{
    public class CreateUnitCommand : IRequest<int>
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
    public UnitAddressDto? UnitAddressDto { get; set; } 
    public UnitContactsDto? UnitContactsDto { get; set;}   
    }



}