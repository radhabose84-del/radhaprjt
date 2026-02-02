using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn
{
    public class IssueReturnDetailDto
    {
            public int Id { get; set; }
            public int ItemId { get; set; }
            public int UomId { get; set; }
            public decimal ReturnQuantity { get; set; }
            public decimal ReturnValue { get; set; }
            public int ReasonId { get; set; }
            public string? Remarks { get; set; }
    }
}