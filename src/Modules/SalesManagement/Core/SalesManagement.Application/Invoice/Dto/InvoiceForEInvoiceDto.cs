namespace SalesManagement.Application.Invoice.Dto
{
    public class InvoiceForEInvoiceDto
    {
        // Invoice Header
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int UnitId { get; set; }
        public int PartyId { get; set; }
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
        public decimal TCS { get; set; }
        public decimal RoundOff { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string? Remarks { get; set; }

        // From Party (cross-module lookup)
        public string? GstNo { get; set; }
        public bool ReverseCharge { get; set; }
        public string? PlaceOfSupply { get; set; }

        // From DispatchAdvice (transport details for EWaybill)
        public int? TransporterId { get; set; }
        public string? TransporterGstin { get; set; }
        public string? TransporterName { get; set; }
        public string? VehicleNo { get; set; }
        public int? TransportMode { get; set; }

        // From SalesOrder
        public int? SalesOrderTypeId { get; set; }

        // Detail items
        public List<InvoiceDetailForEInvoiceDto> Details { get; set; } = new();
    }

    public class InvoiceDetailForEInvoiceDto
    {
        public int ItemSno { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? HsnCode { get; set; }
        public decimal NoOfBags { get; set; }
        public decimal BagWeight { get; set; }
        public decimal NetWeight { get; set; }
        public decimal RatePerKg { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal FreightValue { get; set; }
        public decimal CommissionValue { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal GstPercentage { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal Charity { get; set; }
        public decimal HandlingCharges { get; set; }
        public decimal TotalAmount { get; set; }
        public int? PackTypeId { get; set; }
        public int? UOMId { get; set; }
        public string? UOMName { get; set; }
    }
}
