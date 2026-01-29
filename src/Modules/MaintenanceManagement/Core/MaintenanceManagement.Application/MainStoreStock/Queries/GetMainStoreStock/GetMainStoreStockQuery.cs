using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MainStoreStock.Queries.GetMainStoreStock
{
    public class GetMainStoreStockQuery : IRequest<List<MainStoresStockDto>>
    {
        public string? OldUnitcode { get; set; }
        public string? GroupCode { get; set; }
    }
}