using MediatR;

namespace MaintenanceManagement.Application.MainStoreStock.Queries.GetMainStoreStock
{
    public class GetMainStoreStockQuery : IRequest<List<MainStoresStockDto>>
    {
        public string? OldUnitcode { get; set; }
        public string? GroupCode { get; set; }
    }
}