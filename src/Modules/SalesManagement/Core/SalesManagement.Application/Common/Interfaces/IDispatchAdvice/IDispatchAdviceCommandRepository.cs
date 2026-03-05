using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.IDispatchAdvice
{
    public interface IDispatchAdviceCommandRepository
    {
        Task<string> GenerateNextDispatchNoAsync(int unitId, CancellationToken ct = default);
        Task<int> CreateAsync(DispatchAdviceHeader entity);
        Task<int> UpdateAsync(DispatchAdviceHeader entity);
    }
}
