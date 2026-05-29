namespace PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Create;

public class BlanketPOWorkFlowDto
{
    public int Id { get; set; }
    public string? PONumber { get; set; }
    public int VendorId { get; set; }
    public int StatusId { get; set; }
    public int UnitId { get; set; }
}
