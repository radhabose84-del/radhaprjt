namespace PurchaseManagement.Application.ContractPO.Dto;

public class ContractPOLookupDto
{
    public int Id { get; set; }
    public string? ContractPONumber { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public DateTimeOffset ValidityFrom { get; set; }
    public DateTimeOffset ValidityTo { get; set; }
    public decimal BalanceValue { get; set; }
    public string? StatusName { get; set; }
}
