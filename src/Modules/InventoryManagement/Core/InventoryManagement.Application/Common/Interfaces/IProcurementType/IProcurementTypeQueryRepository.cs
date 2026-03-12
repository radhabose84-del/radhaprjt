using InventoryManagement.Application.ProcurementType.Dto;

namespace InventoryManagement.Application.Common.Interfaces.IProcurementType
{
    public interface IProcurementTypeQueryRepository
    {
        Task<(List<ProcurementTypeDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<ProcurementTypeDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<ProcurementTypeLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<string> GetNextProcurementCodeAsync(string prefix);
    }
}
