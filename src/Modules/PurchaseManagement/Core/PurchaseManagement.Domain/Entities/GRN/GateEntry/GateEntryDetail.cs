using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.PurchaseOrder;

namespace PurchaseManagement.Domain.Entities.GRN.GateEntry
{
    public class GateEntryDetail
    {
        public int Id { get; set; }
        public int GateEntryHeaderId { get; set; }
        public GateEntryHeader GateEntryHeaderDetails { get; set; } = null!;
        public int PoCategoryId { get; set; }
        public MiscMaster? PoType { get; set; }
        public int PoMethodId { get; set; }
        public MiscMaster? PoGateMethodDetails { get; set; }
        public int PoId { get; set; }
        public PurchaseOrderHeader GatePurchaseOrder { get; set; } = null!;
        public DateTimeOffset PoDate { get; set; }
        public string PoCreatedBy { get; set; } = string.Empty;
        public string? GSTNumber { get; set; }
        public string? ContactDetails { get; set; }
     
    }
}