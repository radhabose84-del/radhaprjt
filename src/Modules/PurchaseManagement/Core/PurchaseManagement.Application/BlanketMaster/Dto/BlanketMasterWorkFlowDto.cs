namespace PurchaseManagement.Application.BlanketMaster.Dto;

public sealed class BlanketMasterWorkFlowDto
{
    public int Id { get; set; }
    public string? BlanketNumber { get; set; }
    public int VendorId { get; set; }
    public int StatusId { get; set; }
    public int UnitId { get; set; }
}
