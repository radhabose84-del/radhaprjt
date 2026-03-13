namespace FinanceManagement.Application.Common.Interfaces.IEWaybillHeader
{
    public interface IEWaybillHeaderCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.EWaybillHeader entity);
        Task<int> UpdateAsync(Domain.Entities.EWaybillHeader entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
