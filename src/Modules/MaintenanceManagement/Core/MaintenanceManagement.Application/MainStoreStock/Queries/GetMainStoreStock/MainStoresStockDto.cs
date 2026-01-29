using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.MainStoreStock.Queries.GetMainStoreStock
{
    public class MainStoresStockDto
    {
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Uom { get; set; }
        public decimal StockQty { get; set; }
        public decimal StockValue { get; set; }
    }
}