using System.Data;
using Contracts.Dtos.Lookups.Finance;
namespace Contracts.Interfaces.Lookups.Finance
{
    public interface IDocumentSequenceLookup
    {
        Task<int?> GetTransactionTypeIdAsync(string typeName, string moduleName, int unitId);
        Task<IReadOnlyList<string>> GenerateDocumentNumber(int transactionTypeId);
        Task<List<TransactionTypeLookupDto>> GetTransactionTypesForYearAsync(int financialYearId, int unitId, int? moduleId = null, int? menuId = null, CancellationToken ct = default);
 		Task IncrementDocNoAsync(int transactionTypeId, IDbConnection connection, IDbTransaction transaction);
        Task<IReadOnlyList<TransactionTypeLookupDto>> GetTransactionTypesByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
