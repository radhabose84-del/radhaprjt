namespace FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster
{
    public interface ITransactionTypeMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.TransactionTypeMaster entity);
        Task<int> UpdateAsync(Domain.Entities.TransactionTypeMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
