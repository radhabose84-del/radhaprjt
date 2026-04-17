using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Application.DeliveryChallan.Queries.GetDCGatePassPending;

namespace SalesManagement.Application.Common.Interfaces.IDeliveryChallan
{
    public interface IDeliveryChallanQueryRepository
    {
        Task<(List<DeliveryChallanHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<DeliveryChallanHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<DeliveryChallanLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<IReadOnlyList<DeliveryChallanLookupDto>> GetForReceiptAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> StoHeaderExistsAsync(int stoHeaderId);
        Task<bool> LotExistsAsync(int lotId);
        Task<StoOpenQtyDto?> GetStoOpenQtyAsync(int stoDetailId);
        Task<bool> HasStoReceiptAsync(int dcHeaderId);
        Task<bool> IsStoApprovedAsync(int stoHeaderId);
        Task<bool> IsStoFullyDispatchedAsync(int stoHeaderId);
        Task<(List<DeliveryChallanHeaderDto>, int)> GetPendingAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<DeliveryChallanHeaderDto?> GetPendingByIdAsync(int id);
        Task<List<GetDCGatePassPendingDto>> GetDCGatePassPendingAsync(string? vehicleNo);
    }
}
