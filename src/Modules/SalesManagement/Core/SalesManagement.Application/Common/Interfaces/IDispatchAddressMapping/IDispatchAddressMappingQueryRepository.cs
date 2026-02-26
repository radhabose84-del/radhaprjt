using SalesManagement.Application.DispatchAddressMapping.Dto;

namespace SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping
{
    public interface IDispatchAddressMappingQueryRepository
    {
        Task<(List<DispatchAddressMappingDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? partyId = null);
        Task<DispatchAddressMappingDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<DispatchAddressMappingLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> CompositeKeyExistsAsync(int partyId, int dispatchAddressId, int usageTypeId, int? excludeId = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> DispatchAddressExistsAsync(int dispatchAddressId);
        Task<bool> MiscMasterExistsAsync(int usageTypeId);
        Task<bool> DefaultAlreadyExistsAsync(int partyId, int usageTypeId, int? excludeId = null);
    }
}
