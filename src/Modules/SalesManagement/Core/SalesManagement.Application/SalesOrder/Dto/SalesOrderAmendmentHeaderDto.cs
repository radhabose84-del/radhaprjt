namespace SalesManagement.Application.SalesOrder.Dto
{
    public class SalesOrderAmendmentHeaderDto
    {
        public int Id { get; set; }
        public int SalesOrderHeaderId { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? SalesOrderNo { get; set; }
        public string? AmendmentNo { get; set; }
        public int RevisionNumber { get; set; }
        public DateOnly AmendmentDate { get; set; }
        public string? Reason { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedDate { get; set; }

        // Agent Commission snapshot
        public int? AgentCommissionId { get; set; }
        public int? AgentCommissionSlabId { get; set; }
        public int AgentPaymentTermsId { get; set; }
        public decimal? CommissionRate { get; set; }
        public decimal? CommissionValue { get; set; }

        // Discount snapshot
        public decimal? MdDiscountValue { get; set; }
        public decimal? TotalDiscountValue { get; set; }

        // Audit
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }

        public List<SalesOrderAmendmentDetailDto>? SalesOrderAmendmentDetails { get; set; }

        // Applied discounts snapshot
        public List<SalesOrderAmendmentDiscountDto>? Discounts { get; set; }
    }
}
