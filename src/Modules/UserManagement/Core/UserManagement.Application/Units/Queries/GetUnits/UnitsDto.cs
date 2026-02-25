namespace UserManagement.Application.Units.Queries.GetUnits
{
    public class UnitsDto 
    {
    public int Id { get; set; }
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