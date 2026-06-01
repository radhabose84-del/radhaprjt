namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

public class PurchaseReturnGrnLookupDto
{
    public int GrnHeaderId { get; set; }
    public string? GrnNo { get; set; }
    public DateTimeOffset GrnDate { get; set; }
}
