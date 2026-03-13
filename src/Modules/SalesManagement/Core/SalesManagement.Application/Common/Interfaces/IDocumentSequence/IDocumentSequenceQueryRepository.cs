namespace SalesManagement.Application.Common.Interfaces.IDocumentSequence
{
    /// <summary>
    /// Cross-module read interface — queries Finance.DocumentSequence for document number generation.
    /// SalesManagement handlers use this for auto-generating doc numbers (Invoice, DC, STO, etc.)
    /// </summary>
    public interface IDocumentSequenceQueryRepository
    {
        Task<int?> GetTransactionTypeIdAsync(string typeName, string moduleName, int unitId);
        Task<IReadOnlyList<string>> GenerateDocumentNumber(int transactionTypeId);
    }
}
