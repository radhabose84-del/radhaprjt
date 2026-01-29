using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Inventory
{
    public class UOMMasterDto
    {
        
        public int Id { get; set; }
        public string Code { get; set; }
        public string UOMName { get; set; }        
        public int UOMTypeId { get; set; }
        public bool IsActive { get; set; }

    }
}