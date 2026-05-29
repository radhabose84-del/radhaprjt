using PurchaseManagement.Application.BlanketMaster.Dto;

namespace PurchaseManagement.Application.BlanketMaster.Commands.Create;

public class CreateBlanketMasterReverseDto
{
    public BlanketMasterWorkFlowDto? Header { get; set; }
    public ICollection<BlanketMasterWorkFlowDto>? Lines { get; set; }
}
