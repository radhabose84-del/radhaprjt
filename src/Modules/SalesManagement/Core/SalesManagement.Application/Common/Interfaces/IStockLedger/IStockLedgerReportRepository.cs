using SalesManagement.Application.StockLedger.Dto;

namespace SalesManagement.Application.Common.Interfaces.IStockLedger
{
    public interface IStockLedgerReportRepository
    {
        Task<(List<StockLedgerReportDto>, int)> GetReportAsync(
            int pageNumber,
            int pageSize,
            int? itemId,
            int? lotId,
            int? warehouseId,
            int? binId,
            int? statusId,
            int? packNo,
            DateOnly? dateFrom,
            DateOnly? dateTo);
    }
}
