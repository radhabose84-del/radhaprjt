using SalesManagement.Application.StoReceipt.Dto;

namespace SalesManagement.Application.Common.Interfaces.IStoReceipt
{
    public interface IStoReceiptQueryRepository
    {
        Task<(List<StoReceiptHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<StoReceiptHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<StoReceiptLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> DeliveryChallanHeaderExistsAsync(int dcHeaderId);
        Task<DcOpenQtyDto?> GetDcOpenQtyAsync(int dcDetailId);
        Task<bool> IsDcApprovedAsync(int dcHeaderId);
        Task<bool> IsStoReceiptExistsForDcAsync(int dcHeaderId);
    }
}
