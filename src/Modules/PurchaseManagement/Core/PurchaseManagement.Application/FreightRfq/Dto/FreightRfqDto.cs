namespace PurchaseManagement.Application.FreightRfq.Dto
{
    /// <summary>Full Freight RFQ projection (header + quotations + derived stats) for view / comparison.</summary>
    public class FreightRfqDto
    {
        public int Id { get; set; }
        public string? FreightRfqNumber { get; set; }
        public DateTimeOffset RfqDate { get; set; }
        public DateTimeOffset? RfqValidTill { get; set; }

        public int RfqTypeId { get; set; }
        public string? RfqTypeName { get; set; }

        public int? PoReferenceId { get; set; }
        public string? PoNumber { get; set; }

        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }     // resolved via ISupplierLookup

        public string? SourceLocation { get; set; }
        public string? SourceStation { get; set; }
        public string? DestinationLocation { get; set; }
        public string? DestinationStation { get; set; }

        public decimal TotalQuantity { get; set; }
        public int TotalBaleCount { get; set; }

        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public int? ApprovalRequestHeaderId { get; set; }   // workflow ApprovalRequest.Id (for the approve call)

        public int? SelectedQuotationId { get; set; }
        public string? ComparisonRemarks { get; set; }

        public int? ApprovedTransporterId { get; set; }
        public string? ApprovedTransporterName { get; set; }
        public decimal? ApprovedRate { get; set; }
        public decimal? ApprovedFreightValue { get; set; }
        public string? ApprovalRemarks { get; set; }

        public bool IsActive { get; set; }

        // Derived (computed, not stored)
        public decimal? LowestQuotedRate { get; set; }
        public decimal? HighestQuotedRate { get; set; }
        public decimal? VarianceAmount { get; set; }   // selected freight - lowest freight

        public List<FreightRfqQuotationDto> Quotations { get; set; } = new();
    }
}
