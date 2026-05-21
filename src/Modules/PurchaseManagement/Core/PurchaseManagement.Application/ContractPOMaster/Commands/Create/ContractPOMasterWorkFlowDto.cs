namespace PurchaseManagement.Application.ContractPOMaster.Commands.Create;

public class ContractPOMasterWorkFlowDto
{
    public int Id { get; set; }
    public string? ContractPONumber { get; set; }
    public int VendorId { get; set; }
    public int StatusId { get; set; }
    public int UnitId { get; set; }
}
