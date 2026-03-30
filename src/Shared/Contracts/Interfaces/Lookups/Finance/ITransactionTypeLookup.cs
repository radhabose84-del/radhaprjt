using Contracts.Dtos.Lookups.Finance;

namespace Contracts.Interfaces.Lookups.Finance
{
    public interface ITransactionTypeLookup
    {
        Task<IReadOnlyList<TransactionTypeLookupDto>> GetAllTransactionTypeAsync();
        Task<IReadOnlyList<TransactionTypeLookupDto>> GetByIdsAsync(IEnumerable<int> ids);
    }
}
