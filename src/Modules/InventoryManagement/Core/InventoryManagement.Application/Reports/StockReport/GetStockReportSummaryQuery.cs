using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace InventoryManagement.Application.Reports.StockReport
{
    public class GetStockReportSummaryQuery : IRequest<List<StockSummaryDto>>
    {
        
        public int? ItemId { get; set; }
        public int? WarehouseId { get; set; }
        public int? StorageTypeId { get; set; }
        public int? TargetId { get; set; }
    }
}