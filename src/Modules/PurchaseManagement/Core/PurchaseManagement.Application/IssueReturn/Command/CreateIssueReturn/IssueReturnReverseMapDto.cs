using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn.CreateIssueReturnDto;

namespace PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn
{
    public class IssueReturnReverseMapDto
    {
         // Represents the header information (one record)
        public IssueReturnHeaderDto? Header { get; set; }

        // Represents the detail rows (many records)
        public ICollection<IssueReturnDetailDto>? Lines { get; set; } 
    }
}