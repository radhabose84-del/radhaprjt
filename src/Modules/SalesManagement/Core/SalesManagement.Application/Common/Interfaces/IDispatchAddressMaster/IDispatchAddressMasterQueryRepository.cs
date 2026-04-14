using SalesManagement.Application.DispatchAddressMaster.Dto;

namespace SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster
{
    public interface IDispatchAddressMasterQueryRepository
    {
        Task<(List<DispatchAddressMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<DispatchAddressMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<DispatchAddressMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> CompositeKeyExistsAsync(string name, int cityId, string pinCode, int? excludeId = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> CityExistsAsync(int cityId);
        Task<bool> StateExistsAsync(int stateId);
        Task<bool> CountryExistsAsync(int countryId);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsDispatchAddressMasterLinkedAsync(int id);
    }
}
