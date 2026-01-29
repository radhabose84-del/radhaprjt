using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.MainStoreStock.Queries.GetItemStockbyId
{
    public class MainStoreItemStockDto
    {
        public decimal StockQty { get; set; }
        public decimal StockValue { get; set; }
        public decimal Rate { get; set; }
    }
}