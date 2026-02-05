using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Domain.Common;

namespace InventoryManagement.Domain.Entities
{
    public class UOMConversion : BaseEntity
    {

        public int  FromUOMId { get; set; }
        public int  ToUOMId { get; set; }
        public decimal ConversionValue { get; set; }
        
          // ✅ Navigation Properties
        public UOM? FromUOM { get; set; }
        public UOM? ToUOM { get; set; }
    }
}