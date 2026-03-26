namespace SalesManagement.Application.Invoice.Queries.GetInvoiceGatePassPending
{
    public class GetInvoiceGatePassPendingDto
    {
        // Header fields
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int InvoiceType { get; set; }
        public string? InvoiceTypeName { get; set; }
        public int DispatchAdviceId { get; set; }
        public string? DispatchNo { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? VehicleNumber { get; set; }
        public string? TransporterName { get; set; }
        public string? LRNumber { get; set; }
        public DateOnly? LRDate { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string? Remarks { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }

        // Detail lines
        public List<GetInvoiceGatePassPendingDetailDto> InvoiceDetails { get; set; } = new();

        public class GetInvoiceGatePassPendingDetailDto
        {
            public int InvoiceId { get; set; }
            public int ItemId { get; set; }
            public string? ItemName { get; set; }
            public string? HsnCode { get; set; }
            public int NoOfBags { get; set; }
            public decimal Quantity { get; set; }
            public decimal RatePerKg { get; set; }
            public decimal TaxableAmount { get; set; }
            public decimal TaxAmount { get; set; }
            public decimal TotalAmount { get; set; }
        }
    }
}
