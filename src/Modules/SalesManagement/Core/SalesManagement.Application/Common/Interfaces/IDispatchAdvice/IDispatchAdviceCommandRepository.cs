using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.IDispatchAdvice
{
    public interface IDispatchAdviceCommandRepository
    {
        Task<int> CreateAsync(
            DispatchAdviceHeader entity,
            int unitId,
            int packedStatusId,
            int reservedStatusId,
            int transactionTypeId,
            string? dispatchTypeName,
            string directToPartyName,
            string othersName);
        Task<bool> SoftDeleteAsync(int id, int reservedStatusId, int packedStatusId, CancellationToken ct);
    }
}
