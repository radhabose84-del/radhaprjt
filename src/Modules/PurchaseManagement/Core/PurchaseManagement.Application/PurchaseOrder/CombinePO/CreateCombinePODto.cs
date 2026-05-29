using PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO;
public sealed class CreateCombinePODto
{
    public int POMethodId { get; set; }
    public PurchaseOrderCreateDto? Local { get; set; }
    public ImportPOCreateDto? Import { get; set; }
    public ContractPOCreateDto? Contract { get; set; }
    public BlanketPOCreateDto? Blanket { get; set; }
}

public sealed class AmendCombinePODto
{
    public int PoId { get; set; }
    public int POMethodId { get; set; }
    public PurchaseOrderUpdateDto? Local { get; set; }
    public ImportPOUpdateDto? Import { get; set; }
    public ContractPOUpdateDto? Contract { get; set; }
    public BlanketPOUpdateDto? Blanket { get; set; }
}

public sealed class GetCombinePOByIdVm
{
    public int POMethodId { get; set; }
    public PurchaseOrderDetailDto? Local { get; set; }
    public ImportPOFullVm? Import { get; set; }
    public ContractPODetailVm? Contract { get; set; }
    public BlanketPODetailVm? Blanket { get; set; }
}

public sealed class UpdateCombinePODto
{
    public int POMethodId { get; set; }
    public PurchaseOrderUpdateDto? Local { get; set; }
    public ImportPOUpdateDto? Import { get; set; }
    public ContractPOUpdateDto? Contract { get; set; }
    public BlanketPOUpdateDto? Blanket { get; set; }
}
