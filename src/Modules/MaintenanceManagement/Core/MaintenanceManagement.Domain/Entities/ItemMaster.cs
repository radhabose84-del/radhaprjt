using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Domain.Entities
{
    public class ItemMaster
    {
         public string? ItemCode { get; set; }
         public string? ItemName { get; set; }
         public string? Uom { get; set; }
         public string? Description { get; set; }
    }
}