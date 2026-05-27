using DomainReturnType = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnType;

namespace PurchaseManagement.Application.Common.Interfaces.IReturnType;

public interface IReturnTypeCommandRepository
{
    Task<DomainReturnType> CreateAsync(DomainReturnType entity, CancellationToken ct);
    Task<DomainReturnType> UpdateAsync(DomainReturnType entity, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
}
