using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.MRS.Queries.GetParentWarehouse
{
    public class GetParentWarehouseDto
    {
        public int ParentWarehouseId { get; set; }
        public string? ParentWarehouseName { get; set; }
    }
}