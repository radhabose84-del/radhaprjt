using SalesManagement.Application.SalesChannel.Dto;

namespace SalesManagement.Application.Common.Interfaces.ISalesChannel
{
    public interface ISalesChannelQueryRepository
    {
        Task<(List<SalesChannelDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesChannelDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<SalesChannelLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string salesChannelCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsSalesChannelLinkedAsync(int id);
    }
}
