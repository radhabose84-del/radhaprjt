using DomainReturnReason = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnReason;

namespace PurchaseManagement.Application.Common.Interfaces.IReturnReason;

public interface IReturnReasonCommandRepository
{
    Task<DomainReturnReason> CreateAsync(DomainReturnReason entity, CancellationToken ct);
    Task<DomainReturnReason> UpdateAsync(DomainReturnReason entity, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
}
