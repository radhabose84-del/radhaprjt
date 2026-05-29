namespace PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;

public sealed class BlanketPODetailVm : BlanketPOUpdateDto
{
    public int UnitId { get; set; }
    public new int StatusId { get; set; }
    public string? PONumber { get; set; }
    public string? VendorName { get; set; }
    public string? CurrencyName { get; set; }
    public string? BlanketNumber { get; set; }
    public int VendorId { get; set; }
    public int CurrencyId { get; set; }
}
