using PurchaseManagement.Domain.Entities.ContractPO;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;

public interface IContractPOCommandRepository
{
    Task<ContractPOHeader> CreateAsync(ContractPOHeader entity, int transactionTypeId, CancellationToken ct);
    Task<ContractPOHeader> UpdateAsync(ContractPOHeader entity, List<ContractPODetail> details, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);

    Task<int> CreateCombinePOAsync(
        PurchaseOrderHeader poHeader,
        PurchaseContractHeader contractHeader,
        List<PurchaseContractDetail> contractDetails,
        List<ContractPOReleaseHistory> releaseHistories,
        int transactionTypeId,
        CancellationToken ct);

    Task<int> UpdateContractReleasePOAsync(
        PurchaseOrderHeader poHeader,
        PurchaseContractHeader contractHeader,
        List<PurchaseContractDetail> contractDetails,
        List<ContractPOReleaseHistory> releaseHistories,
        CancellationToken ct);

    Task<int> DeleteContractReleasePOAsync(int poId, CancellationToken ct);

    Task<int> AmendContractReleasePOAsync(
        int existingPoId,
        PurchaseOrderHeader newPoHeader,
        PurchaseContractHeader newContractHeader,
        List<PurchaseContractDetail> newContractDetails,
        List<ContractPOReleaseHistory> newReleaseHistories,
        int transactionTypeId,
        CancellationToken ct);
}
