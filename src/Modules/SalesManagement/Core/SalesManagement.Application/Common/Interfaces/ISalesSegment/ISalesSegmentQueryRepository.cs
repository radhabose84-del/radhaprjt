
using SalesManagement.Application.SalesSegment.Dto;

namespace SalesManagement.Application.Common.Interfaces.ISalesSegment
{
    public interface ISalesSegmentQueryRepository
    {
        Task<(List<SalesSegmentDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesSegmentDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<SalesSegmentLookupDto>> AutocompleteAsync(string term, CancellationToken ct);

        // Validation methods
        Task<bool> CompositeKeyExistsAsync(int salesOrgId, int channelId, int buId, int? excludeId = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SalesOrganisationExistsAsync(int id);
        Task<bool> SalesChannelExistsAsync(int id);
        Task<bool> BusinessUnitExistsAsync(int id);
        Task<bool> SoftDeleteValidationAsync(int id);
    }
}
