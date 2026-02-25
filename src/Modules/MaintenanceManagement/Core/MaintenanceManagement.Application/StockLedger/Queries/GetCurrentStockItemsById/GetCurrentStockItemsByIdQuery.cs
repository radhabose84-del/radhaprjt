using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStockItemsById
{
    public class GetCurrentStockItemsByIdQuery : IRequest<ApiResponseDTO<List<StockItemCodeDto>>>
    {
        public string? OldUnitcode { get; set; }
        public int DepartmentId { get; set; }
    }
}