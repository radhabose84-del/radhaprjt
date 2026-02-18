using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentAutoComplete
{
    public class PurchaseIndentAutoCompleteQueryDto
    {
        public int Id { get; set; }
        public string IndentNumber { get; set; } = default!;
    }
}