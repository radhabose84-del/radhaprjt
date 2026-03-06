using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Application.Common.Interfaces.IDispatchAdvice
{
    public interface IDispatchAdviceQueryRepository
    {
        Task<(List<DispatchAdviceHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<DispatchAdviceHeaderDto?> GetByIdAsync(int id);
        Task<bool> SalesOrderExistsAsync(int salesOrderId);
        Task<bool> DispatchAddressExistsAsync(int dispatchAddressId);
        Task<int> GetSalesOrderUnitIdAsync(int salesOrderId);
        Task<DispatchAdviceStockDto?> GetStockAsync(int itemId, int lotId, int statusId);
        Task<List<int>> GetAvailablePackNosAsync(int itemId, int lotId, int statusId, int startPackNo, int endPackNo);
        Task<IReadOnlyList<DispatchAdviceLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
    }
}
