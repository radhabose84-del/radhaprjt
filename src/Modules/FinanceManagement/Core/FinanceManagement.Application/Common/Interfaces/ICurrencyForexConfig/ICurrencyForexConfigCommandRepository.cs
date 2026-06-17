namespace FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig
{
    public interface ICurrencyForexConfigCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.CurrencyForexConfig entity);
        Task<int> UpdateAsync(Domain.Entities.CurrencyForexConfig entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
