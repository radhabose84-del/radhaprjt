using SalesManagement.Application.TripSheet.Dto;

namespace SalesManagement.Application.Common.Interfaces.ITripSheet
{
    public interface ITripSheetQueryRepository
    {
        Task<(List<TripSheetHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<TripSheetHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<TripSheetLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string tripSheetNo, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> DispatchExistsAsync(int dispatchAdviceHeaderId);
        Task<bool> DispatchAlreadyInTripAsync(int dispatchAdviceHeaderId, int? excludeTripSheetHeaderId = null);
    }
}
