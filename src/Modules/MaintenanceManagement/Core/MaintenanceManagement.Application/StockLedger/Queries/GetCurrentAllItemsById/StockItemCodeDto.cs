using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStockItemsById
{
    public class StockItemCodeDto
    {
        public string? ItemCode { get; set; }

        public string? ItemName { get; set; }
    }
}