using Contracts.Common;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStockItemsById;
using MediatR;

namespace MaintenanceManagement.Application.StockLedger.Queries.GetCurrentAllItemsById
{
    public class GetCurrentAllItemsByIdQuery : IRequest<ApiResponseDTO<List<StockItemCodeDto>>>
    {
        public string? OldUnitcode { get; set; }
        public int DepartmentId { get; set; }
    }
}