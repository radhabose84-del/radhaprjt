using PurchaseManagement.Application.BlanketMaster.Dto;
using PurchaseManagement.Domain.Entities.BlanketMaster;

namespace PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;

public interface IBlanketMasterCommandRepository
{
    Task<BlanketHeader> CreateAsync(BlanketHeader entity, int transactionTypeId, CancellationToken ct);
    Task<BlanketHeader> UpdateAsync(BlanketHeader entity, List<BlanketDetail> details, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    Task<bool> UpdateBlanketApproveAsync(int id, int statusId, CancellationToken ct);
    Task<BlanketMasterWorkFlowDto> GetByIdBlanketWorkFlowAsync(int id);
}
