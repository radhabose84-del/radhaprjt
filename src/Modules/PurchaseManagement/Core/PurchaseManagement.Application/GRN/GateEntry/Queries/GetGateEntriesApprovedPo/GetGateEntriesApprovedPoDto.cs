using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.GRN.GateEntry.Queries.GetGateEntriesApprovedPo
{
    public class GetGateEntriesApprovedPoDto
    {
         // PO Header Info
        public int PoId { get; set; }
        public string? PONumber { get; set; }
        public DateTimeOffset? PoDate { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int VendorId { get; set; }
        public string? VendorName { get; set; }
        public string? GSTNumber { get; set; }
        public string? ContactName { get; set; }
        public int POCategoryId { get; set; }
        public int POMethodId { get; set; }
        public decimal TotalOrderQty { get; set; }
        public decimal TotalGrnQty { get; set; }
        public decimal PendingQty { get; set; }
    }
}