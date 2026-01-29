using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Purchase
{
    public class UpdateLineStatusDto
    {
        public int ModuleLineId { get; set; }
        // public decimal ApprovedQuantity { get; set; }
        public string Status { get; set; }
    }
}