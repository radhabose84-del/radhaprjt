namespace PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Create;

public class CreateBlanketPOReverseDto
{
    public BlanketPOWorkFlowDto? Header { get; set; }
    public ICollection<BlanketPOWorkFlowDto>? Lines { get; set; }
}
