namespace FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster
{
    public interface IAccountTypeMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.AccountTypeMaster entity);
        Task<int> UpdateAsync(Domain.Entities.AccountTypeMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
