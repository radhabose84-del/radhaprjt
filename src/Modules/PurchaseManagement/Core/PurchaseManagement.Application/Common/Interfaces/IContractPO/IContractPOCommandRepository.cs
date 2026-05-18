using PurchaseManagement.Domain.Entities.ContractPO;

namespace PurchaseManagement.Application.Common.Interfaces.IContractPO;

public interface IContractPOCommandRepository
{
    Task<ContractPOHeader> CreateAsync(ContractPOHeader entity, int transactionTypeId, CancellationToken ct);
    Task<ContractPOHeader> UpdateAsync(ContractPOHeader entity, List<ContractPODetail> details, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
}
