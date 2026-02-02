using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPending
{
    public class ValidateToleranceDto
    {
        public int PoId { get; set; }
        public int ItemId { get; set; }
        public int PoSlNo { get; set; }
        public decimal OrderQuantity { get; set; }
        public decimal TotalGrnQty { get; set; }
        public decimal PendingQty { get; set; }
        public bool IsPartialReceiptAllowed { get; set; }
    }
}