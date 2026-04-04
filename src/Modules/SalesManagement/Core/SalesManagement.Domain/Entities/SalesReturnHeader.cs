using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesReturnHeader : BaseEntity
    {
        public string? ReturnNumber { get; set; }
        public DateOnly ReturnDate { get; set; }

        // Same-module FK → Sales.ComplaintHeader
        public int ComplaintHeaderId { get; set; }
        public ComplaintHeader? ComplaintHeader { get; set; }

        // Cross-module FK → PartyManagement
        public int CustomerId { get; set; }

        // Cross-module FK → WarehouseManagement
        public int WarehouseId { get; set; }

        // Cross-module FK → WarehouseManagement
        public int BinId { get; set; }

        // Same-module FK → Sales.MiscMaster (ReturnStatus: Pending/Received/Partially Returned/Fully Returned)
        public int StatusId { get; set; }
        public MiscMaster? Status { get; set; }

        public string? Remarks { get; set; }

        // Child collection
        public ICollection<SalesReturnDetail>? SalesReturnDetails { get; set; }
    }
}
