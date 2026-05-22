using PurchaseManagement.Application.PurchaseOrder.ContractPO.Queries.GetContractPOPending;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPOLocalPending;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetImportPOPending;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Queries.GetCombinePOPending;

public sealed class GetCombinePOPendingVm
{
    public int? POMethodId { get; set; }

    public List<GetPOLocalPendingGroupDto>? LocalItems { get; set; }
    public int LocalTotalCount { get; set; }

    public List<GetPOImportPendingGroupDto>? ImportItems { get; set; }
    public int ImportTotalCount { get; set; }

    public List<GetContractPOPendingGroupDto>? ContractItems { get; set; }
    public int ContractTotalCount { get; set; }
}
