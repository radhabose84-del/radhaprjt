using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class ComplaintResolution : BaseEntity
    {
        // Same-module FK → Sales.ComplaintHeader (one resolution per complaint)
        public int ComplaintHeaderId { get; set; }
        public ComplaintHeader? ComplaintHeader { get; set; }

        // Same-module FK → Sales.MiscMaster (ResolutionType: Sales Return/Credit Note/Replacement/Reprocess/No Action)
        public int ResolutionTypeId { get; set; }
        public MiscMaster? ResolutionType { get; set; }

        public string? ResolutionSummary { get; set; }

        // Sales Return Details (when ResolutionType = Sales Return)
        public decimal? ReturnQuantity { get; set; }

        // Cross-module FK → Warehouse.WarehouseMaster (no DB constraint, validated via IWarehouseLookup)
        public int? ReturnLocationId { get; set; }

        public int? ReturnStatusId { get; set; }
        public MiscMaster? ReturnStatus { get; set; }

        // Credit Note Details (when ResolutionType = Credit Note)
        public decimal? CreditAmount { get; set; }
        public string? FinanceReference { get; set; }

        // Replacement Details (when ResolutionType = Replacement)
        public decimal? ReplacementQuantity { get; set; }
        public string? DispatchReference { get; set; }

        // Reprocess Details (when ResolutionType = Reprocess)
        public string? ActionDescription { get; set; }

        // Closure
        public int? ClosureStatusId { get; set; }
        public MiscMaster? ClosureStatus { get; set; }
        public string? ClosureRemarks { get; set; }

        // Audit
        public int? ResolvedBy { get; set; }
        public DateTimeOffset? ResolvedDate { get; set; }
        public int? ClosedBy { get; set; }
        public DateTimeOffset? ClosedDate { get; set; }
    }
}
