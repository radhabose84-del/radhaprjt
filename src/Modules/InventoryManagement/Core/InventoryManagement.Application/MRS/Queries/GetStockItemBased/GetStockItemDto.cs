using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagement.Application.MRS.Queries.GetStockItemBased
{
    public class GetStockItemDto
    {
          public int ItemId { get; set; }
        public int UomId { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public decimal CurrentStockQty { get; set; }
        public decimal CurrentStockValue { get; set; }
    }
}