namespace SalesManagement.Application.SalesQuotation.Dto
{
    public class SalesQuotationAmendmentHeaderDto
    {
        public int Id { get; set; }
        public int SalesQuotationHeaderId { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? QuotationNo { get; set; }
        public string? AmendmentNo { get; set; }
        public int RevisionNumber { get; set; }
        public DateOnly AmendmentDate { get; set; }
        public string? Reason { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }

        // Header-level Summary Fields
        public decimal FreightCharges { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal TotalBasicAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetTaxableAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrandTotal { get; set; }

        public int? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedDate { get; set; }

        // Audit
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }

        public List<SalesQuotationAmendmentDetailDto>? SalesQuotationAmendmentDetails { get; set; }
    }
}
