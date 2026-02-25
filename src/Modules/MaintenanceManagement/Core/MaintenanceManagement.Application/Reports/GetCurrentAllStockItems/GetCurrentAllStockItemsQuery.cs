using Contracts.Common;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStock;
using MediatR;

namespace MaintenanceManagement.Application.Reports.GetCurrentAllStockItems
{
    public class GetCurrentAllStockItemsQuery : IRequest<ApiResponseDTO<List<CurrentStockDto>>>
    {
        public string? OldUnitcode { get; set; }
        public int DepartmentId { get; set; }
    }
}