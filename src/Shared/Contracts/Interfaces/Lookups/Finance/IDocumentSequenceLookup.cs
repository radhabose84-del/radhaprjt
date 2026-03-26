namespace Contracts.Interfaces.Lookups.Finance
{
    public interface IDocumentSequenceLookup
    {
        Task<int?> GetTransactionTypeIdAsync(string typeName, string moduleName, int unitId);
        Task<IReadOnlyList<string>> GenerateDocumentNumber(int transactionTypeId);
        Task IncrementDocNoAsync(int transactionTypeId);
    }
}
