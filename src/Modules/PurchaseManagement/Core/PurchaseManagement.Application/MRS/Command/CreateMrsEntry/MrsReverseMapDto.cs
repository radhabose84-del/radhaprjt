using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.MRS.Command.CreateMrsEntry
{
    public class MrsReverseMapDto
    {
        // Represents the header information (one record)
        public CreateMrsEntryDto? Header { get; set; }

        // Represents the detail rows (many records)
        public ICollection<CreateMrsDetailDto>? Lines { get; set; } 
    }
}