using MediatR;

namespace MaintenanceManagement.Application.MainStoreStock.Queries.GetMainStoreStockItems
{
    public class GetMainStoreStockItemsQuery : IRequest<List<MainStoresStockItemsDto>>
    {
        
        public string? OldUnitcode { get; set; }
        public string? GroupCode { get; set; }
    }
}