namespace PurchaseManagement.Application.BlanketMaster.Dto;

public class BlanketHeaderDto
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string? BlanketNumber { get; set; }
    public DateTimeOffset BlanketDate { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public int CurrencyId { get; set; }
    public string? CurrencyName { get; set; }
    public int ProcurementTypeId { get; set; }
    public string? ProcurementTypeName { get; set; }
    public string? BrokerName { get; set; }
    public DateTimeOffset ValidityFrom { get; set; }
    public DateTimeOffset ValidityTo { get; set; }
    public int StatusId { get; set; }
    public string? StatusName { get; set; }
    public decimal TotalEstimatedValue { get; set; }
    public string? Remarks { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int CreatedBy { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public string? CreatedByName { get; set; }
    public string? CreatedIP { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
    public string? ModifiedByName { get; set; }
    public string? ModifiedIP { get; set; }

    public List<BlanketDetailDto> Details { get; set; } = new();
}
