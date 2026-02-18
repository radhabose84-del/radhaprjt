using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent
{
    public class IndentReverseMapDto
    {
        public CreateIndentHeaderDto Header { get; set; } = default!;
        public ICollection<CreateIndentDetailDto> Lines { get; set; } = default!;
    }
}