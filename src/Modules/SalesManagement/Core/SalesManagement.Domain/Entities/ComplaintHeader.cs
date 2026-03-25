using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class ComplaintHeader : BaseEntity
    {
        public string? ComplaintNumber { get; set; }
        public DateOnly ComplaintDate { get; set; }

        // Cross-module FK → PartyManagement
        public int CustomerId { get; set; }

        // Customer Snapshot Fields (stored at save)
        public string? CustomerAddress { get; set; }
        public string? CustomerPIN { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPAN { get; set; }
        public string? CustomerGSTNo { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal TotalOS { get; set; }
        public decimal Outstanding { get; set; }
        public decimal BalanceCredit { get; set; }
        public string? Delay { get; set; }
        public string? Ledger { get; set; }

        // Same-module FK → Sales.MiscMaster (ComplaintStatus)
        public int? StatusId { get; set; }
        public MiscMaster? Status { get; set; }

        public string? Remarks { get; set; }

        // Child collection
        public ICollection<ComplaintDetail>? ComplaintDetails { get; set; }
    }
}
