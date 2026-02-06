using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManagement.Application.BinMaster.Queries.GetAllBinMaster
{
    public class BinMasterDto
    {
        public int Id { get; set; }
        public string BinCode { get; set; } = string.Empty;
        public string BinName { get; set; } = string.Empty;

        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }       

        // 🔹 Rack details (nullable if some bins don’t have racks)
        public int? RackId { get; set; }
        public string? RackCode { get; set; }
        public string? RackName { get; set; }

        // 🔹 Capacity + UOM
        public decimal BinCapacity { get; set; }
        public int CapacityUOMId { get; set; }
        public string? CapacityUOMName { get; set; }

        public byte IsActive { get; set; }
    }
}