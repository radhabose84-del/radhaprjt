using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Units.Queries.GetUnits
{
    public class GetUnitsByIdDto
    {
    public int Id { get; set; }
    public string? UnitName { get; set; }
    public string? ShortName { get; set; }
    public int CompanyId { get; set; }
    public string? GstNumber { get; set; }
    public int DivisionId { get; set; }
    public string? UnitHeadName { get; set; }
    public string? CINNO { get; set; }
    public string? OldUnitId { get; set; }
    public Status  IsActive { get; set; }
    public IsDelete IsDeleted { get; set; }
    public bool IsMaintenanceStopStart { get; set; }
    public int? SpindlesCapacity { get; set; }
    public UnitAddressDto? UnitAddressDto { get; set; } 
    public UnitContactsDto? UnitContactsDto { get; set;} 
    }
}