using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.IDispatchAdvice
{
    public interface IDispatchAdviceCommandRepository
    {
        Task<string> GenerateNextDispatchNoAsync(int unitId, CancellationToken ct = default);
        Task<int> CreateAsync(DispatchAdviceHeader entity, int unitId, int packedStatusId, int reservedStatusId);
        Task<bool> SoftDeleteAsync(int id, int reservedStatusId, int packedStatusId, CancellationToken ct);
    }
}
