namespace PurchaseManagement.Application.ContractPOMaster.Commands.Create;

public class CreateContractPOMasterReverseDto
{
    public ContractPOMasterWorkFlowDto? Header { get; set; }
    public ICollection<ContractPOMasterWorkFlowDto>? Lines { get; set; }
}
