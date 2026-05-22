
using PurchaseManagement.Application.ContractPOMaster.Commands.Create;
using PurchaseManagement.Domain.Entities.ContractPOMaster;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;

public interface IContractPOMasterCommandRepository
{
    Task<ContractPOHeader> CreateAsync(ContractPOHeader entity, int transactionTypeId, CancellationToken ct);
    Task<ContractPOHeader> UpdateAsync(ContractPOHeader entity, List<ContractPODetail> details, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    Task<bool> UpdateContractPOApproveAsync(int id, int statusId, CancellationToken ct);
    Task<ContractPOMasterWorkFlowDto> GetByIdContractPOWorkFlowAsync(int id);
}
