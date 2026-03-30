namespace SalesManagement.Application.ComplaintResolution.Dto
{
    public class ComplaintResolutionDto
    {
        public int Id { get; set; }
        public int ComplaintHeaderId { get; set; }
        public string? ComplaintNumber { get; set; }
        public DateOnly? ComplaintDate { get; set; }
        public string? CustomerName { get; set; }
        public string? ItemName { get; set; }
        public decimal? ComplaintQuantity { get; set; }

        // Resolution
        public int ResolutionTypeId { get; set; }
        public string? ResolutionTypeName { get; set; }
        public string? ResolutionSummary { get; set; }

        // Sales Return
        public decimal? ReturnQuantity { get; set; }
        public int? ReturnLocationId { get; set; }
        public string? ReturnLocationName { get; set; }
        public int? ReturnStatusId { get; set; }
        public string? ReturnStatusName { get; set; }

        // Credit Note
        public decimal? CreditAmount { get; set; }
        public string? FinanceReference { get; set; }

        // Replacement
        public decimal? ReplacementQuantity { get; set; }
        public string? DispatchReference { get; set; }

        // Reprocess
        public string? ActionDescription { get; set; }

        // Closure
        public int? ClosureStatusId { get; set; }
        public string? ClosureStatusName { get; set; }
        public string? ClosureRemarks { get; set; }

        // Audit
        public int? ResolvedBy { get; set; }
        public string? ResolvedByName { get; set; }
        public DateTimeOffset? ResolvedDate { get; set; }
        public int? ClosedBy { get; set; }
        public string? ClosedByName { get; set; }
        public DateTimeOffset? ClosedDate { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
