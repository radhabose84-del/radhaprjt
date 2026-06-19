namespace FinanceManagement.Application.Common.Interfaces.ICostCentre
{
    public interface ICostCentreCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.CostCentre entity);
        Task<int> UpdateAsync(Domain.Entities.CostCentre entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
