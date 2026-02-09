using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace WarehouseManagement.Domain.Entities
{
    public class BinMaster : BaseEntity
    {
         public int WarehouseId { get; set; }
        public int? RackId { get; set; }                     // optional        
        public string BinCode { get; set; } = default!;             
        public string? BinName { get; set; }        
        public decimal BinCapacity { get; set; }             
        public int CapacityUOMId { get; set; }
        // Navigations
        public WarehouseMaster Warehouse { get; set; } = default!;
        public RackMaster? Rack { get; set; }

    }
}