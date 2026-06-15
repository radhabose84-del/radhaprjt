namespace FinanceManagement.Application.Common.Interfaces.IGlAccountMaster
{
    public interface IGlAccountMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.GlAccountMaster entity);
        Task<int> UpdateAsync(Domain.Entities.GlAccountMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
