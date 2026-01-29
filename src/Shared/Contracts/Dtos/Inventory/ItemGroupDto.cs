using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Inventory
{
    public class ItemGroupDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string ItemGroupCode { get; set; }
        public string ItemGroupName { get; set; }
        public bool IsActive { get; set; }       
       
    }
}