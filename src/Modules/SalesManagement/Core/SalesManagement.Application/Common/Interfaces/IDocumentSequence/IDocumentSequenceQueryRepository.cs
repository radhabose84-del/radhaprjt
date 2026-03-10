using SalesManagement.Application.DocumentSequence.Dto;

namespace SalesManagement.Application.Common.Interfaces.IDocumentSequence
{
    public interface IDocumentSequenceQueryRepository
    {
        Task<(List<DocumentSequenceDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<DocumentSequenceDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<DocumentSequenceLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<IReadOnlyList<string>> GenerateDocumentNumber(int Id);
        Task<bool> CompositeKeyExistsAsync(int typeId, int financialYearId, int docNo, int? excludeId = null);
        Task<bool> TransactionTypeIdExistsAsync(int typeId);
        Task<bool> FinancialYearExistsAsync(int financialYearId);
        Task<bool> NotFoundAsync(int id);
        Task<int?> GetTransactionTypeIdAsync(string typeName, string moduleName, int unitId);
    }
}
