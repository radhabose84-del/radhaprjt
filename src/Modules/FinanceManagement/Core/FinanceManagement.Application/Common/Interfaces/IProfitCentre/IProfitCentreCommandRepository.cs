namespace FinanceManagement.Application.Common.Interfaces.IProfitCentre
{
    public interface IProfitCentreCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.ProfitCentre entity);
        Task<int> UpdateAsync(Domain.Entities.ProfitCentre entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
