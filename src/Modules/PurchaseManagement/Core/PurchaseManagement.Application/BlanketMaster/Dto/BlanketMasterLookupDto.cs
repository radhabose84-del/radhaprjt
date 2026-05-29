namespace PurchaseManagement.Application.BlanketMaster.Dto;

public class BlanketMasterLookupDto
{
    public int Id { get; set; }
    public string? BlanketNumber { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public DateTimeOffset ValidityFrom { get; set; }
    public DateTimeOffset ValidityTo { get; set; }
    public decimal TotalEstimatedValue { get; set; }
    public string? StatusName { get; set; }
}
