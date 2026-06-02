using UserManagement.Domain.Common;


namespace UserManagement.Domain.Entities
{

public class Unit : BaseEntity
{
public int Id { get; set; }

public string? UnitName { get; set; }

public string? ShortName { get; set; }
public int CompanyId { get; set; }
public Company? Company { get; set; }
public int DivisionId { get; set; }
public Division? Division { get; set; }
public string? UnitHeadName { get; set; }
public string? CINNO {get; set;}
public string? OldUnitId { get; set; }
public bool IsMaintenanceStopStart { get; set;}
public  UnitAddress? UnitAddress { get; set; }
public  UnitContacts? UnitContacts { get; set; }
public IList<UserUnit>? UserUnits { get; set; }
public IList<CustomFieldUnit>? CustomFieldUnits { get; set; }
public int? SpindlesCapacity { get; set; }
public int UnitTypeId { get; set; }
public MiscMaster? UnitType { get; set; }
public string? UnitTypeName { get; set; }
public int? PinCode { get; set; }
public int? BankAccountId { get; set; }

}
}
