using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.IDispatchAdvice
{
    public interface IDispatchAdviceCommandRepository
    {
        Task<string> GenerateNextDispatchNoAsync(int unitId, CancellationToken ct = default);
        Task<int> CreateAsync(DispatchAdviceHeader entity, int unitId, int packedStatusId, int dispatchedStatusId);
        Task<bool> SoftDeleteAsync(int id, int dispatchedStatusId, int packedStatusId, CancellationToken ct);
    }
}
