using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class ComplaintDetail : BaseEntity
    {
        public int ComplaintHeaderId { get; set; }

        // Same-module FK → Sales.InvoiceHeader
        public int InvoiceHeaderId { get; set; }
        public InvoiceHeader? InvoiceHeader { get; set; }

        public DateOnly InvoiceDate { get; set; }

        // Same-module FK → Sales.MiscMaster (InvoiceType)
        public int InvoiceTypeId { get; set; }
        public MiscMaster? InvoiceTypeMisc { get; set; }

        // Cross-module FK → Production.LotMaster
        public int? LotId { get; set; }

        // Cross-module FK → InventoryManagement
        public int ItemId { get; set; }

        public int NumberOfPacks { get; set; }
        public decimal NetWeight { get; set; }
        public decimal InvoiceAmount { get; set; }

        // Cross-module FK → UserManagement.Division
        public int? DivisionId { get; set; }

        // Parent navigation
        public ComplaintHeader? ComplaintHeader { get; set; }

        // Child collection
        public ICollection<ComplaintDetailNature>? ComplaintDetailNatures { get; set; }
    }
}
