using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MainStoreStock.Queries.GetMainStoreStockItems
{
    public class GetMainStoreStockItemsQuery : IRequest<List<MainStoresStockItemsDto>>
    {
        
        public string? OldUnitcode { get; set; }
        public string? GroupCode { get; set; }
    }
}