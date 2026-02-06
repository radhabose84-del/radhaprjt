using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManagement.Application.WarehouseMaster.Queries.GetParentWarehouseMaster
{
    public class GetParentWarehouseDto
    {

        public int Id { get; set; }
        public string? ParentWarehouseCode  { get; set; }
        public string? ParentWarehouseName { get; set; }
    }
}