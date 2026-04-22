namespace SalesManagement.Application.Invoice.Queries.GetInvoicePending
{
    public class GetInvoicePendingDto
    {
        // Header fields
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int? InvoiceTypeId { get; set; }
        public string? InvoiceTypeName { get; set; }
        public int DispatchAdviceId { get; set; }
        public string? DispatchNo { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int FinancialYearId { get; set; }
        public string? FinancialYearName { get; set; }
        public int? TransportMode { get; set; }
        public string? TransportModeName { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? VehicleNumber { get; set; }
        public string? TransporterName { get; set; }
        public string? LRNumber { get; set; }
        public DateOnly? LRDate { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalFreight { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal Insurance { get; set; }
        public decimal HandlingCharge { get; set; }
        public decimal TotalCharity { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TCSPercentage { get; set; }
        public decimal TCS { get; set; }
        public decimal RoundOff { get; set; }
        public decimal InvoiceAmountBeforeTCS { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string? Remarks { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }

        // Workflow fields
        public int ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public int ApprovalRequestHeaderId { get; set; }
        public byte IsEdit { get; set; }

        // Detail lines
        public List<GetInvoicePendingDetailDto> InvoiceDetails { get; set; } = new();

        public class GetInvoicePendingDetailDto
        {
            public int InvoiceId { get; set; }
            public int ItemSno { get; set; }
            public int ItemId { get; set; }
            public string? ItemName { get; set; }
            public string? HsnCode { get; set; }
            public decimal GstPercentage { get; set; }
            public int? LotId { get; set; }
            public string? LotNo { get; set; }
            public int NoOfBags { get; set; }
            public decimal Quantity { get; set; }
            public decimal RatePerKg { get; set; }
            public decimal DiscountValue { get; set; }
            public decimal FreightValue { get; set; }
            public decimal CommissionValue { get; set; }
            public decimal TaxableAmount { get; set; }
            public decimal CgstPercentage { get; set; }
            public decimal SgstPercentage { get; set; }
            public decimal IgstPercentage { get; set; }
            public decimal CGST { get; set; }
            public decimal SGST { get; set; }
            public decimal IGST { get; set; }
            public decimal TaxAmount { get; set; }
            public int? PackTypeId { get; set; }
            public string? PackTypeName { get; set; }
            public int? UOMId { get; set; }
            public string? UOMName { get; set; }
            public decimal Charity { get; set; }
            public decimal HandlingCharges { get; set; }
            public decimal TotalAmount { get; set; }
        }
    }
}
