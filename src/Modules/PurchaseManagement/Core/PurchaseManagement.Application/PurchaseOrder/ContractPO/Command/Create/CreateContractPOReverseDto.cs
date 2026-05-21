namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Create;

public class CreateContractPOReverseDto
{
    public ContractPOWorkFlowDto? Header { get; set; }
    public ICollection<ContractPOWorkFlowDto>? Lines { get; set; }
}
