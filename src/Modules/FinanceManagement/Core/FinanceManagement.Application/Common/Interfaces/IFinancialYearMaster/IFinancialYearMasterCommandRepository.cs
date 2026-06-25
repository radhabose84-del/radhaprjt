namespace FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster
{
    public interface IFinancialYearMasterCommandRepository
    {
        Task<int> CreateAsync(
            Domain.Entities.FinancialYearMaster year,
            IReadOnlyList<Domain.Entities.FinancialPeriodMaster> periods,
            CancellationToken ct);

        Task<int> UpdateAsync(Domain.Entities.FinancialYearMaster entity);

        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
