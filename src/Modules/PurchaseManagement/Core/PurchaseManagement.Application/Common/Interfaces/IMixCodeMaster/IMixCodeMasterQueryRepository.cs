using PurchaseManagement.Application.MixCodeMaster.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster
{
    public interface IMixCodeMasterQueryRepository
    {
        Task<(List<MixCodeMasterDto> Items, int Total)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<MixCodeMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<MixCodeMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string mixCode, int? id = null);
        Task<bool> NotFoundAsync(int id);

        // Rule #25 — ArrivalDetail dependent guard (same-module, direct Dapper EXISTS)
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsMixCodeMasterLinkedAsync(int id);
    }
}
