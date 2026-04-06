using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesReturnDetail : BaseEntity
    {
        // Same-module FK → Sales.SalesReturnHeader
        public int SalesReturnHeaderId { get; set; }
        public SalesReturnHeader? SalesReturnHeader { get; set; }

        // Same-module FK → Sales.InvoiceHeader
        public int InvoiceHeaderId { get; set; }
        public InvoiceHeader? InvoiceHeader { get; set; }

        // Same-module FK → Sales.InvoiceDetail
        public int InvoiceDetailId { get; set; }

        // Cross-module FK → InventoryManagement
        public int ItemId { get; set; }

        // Cross-module FK → ProductionManagement
        public int? LotId { get; set; }

        // Pack range
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }

        // Auto-calculated
        public decimal ReturnQty { get; set; }

        // Cross-module FK → ProductionManagement
        public int? PackTypeId { get; set; }

        // Same-module FK → Sales.MiscMaster (BagStatus: Defect/Damaged)
        public int BagStatusId { get; set; }
        public MiscMaster? BagStatus { get; set; }
    }
}
