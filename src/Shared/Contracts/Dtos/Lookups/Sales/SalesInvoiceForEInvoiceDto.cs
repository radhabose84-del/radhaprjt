namespace Contracts.Dtos.Lookups.Sales
{
    public class SalesInvoiceForEInvoiceDto
    {
        // Invoice Header
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int UnitId { get; set; }
        public int PartyId { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal Discount { get; set; }
        public decimal Freight { get; set; }
        public decimal Insurance { get; set; }
        public decimal HandlingCharge { get; set; }
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

        // From DispatchAdvice / InvoiceHeader (transport details for EWaybill)
        public int? TransporterId { get; set; }
        public string? TransporterGstin { get; set; }
        public string? TransporterName { get; set; }
        public string? VehicleNo { get; set; }
        public int? TransportMode { get; set; }
        /// <summary>NIC transport mode code ("1"=Road, "2"=Rail, "3"=Air, "4"=Ship) — mapped from MiscMaster.Code</summary>
        public string? TransportModeCode { get; set; }

        // From SalesOrder
        public int? SalesOrderTypeId { get; set; }

        // Detail items
        public List<SalesInvoiceDetailForEInvoiceDto> Details { get; set; } = new();
    }

    public class SalesInvoiceDetailForEInvoiceDto
    {
        public int ItemSno { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? HsnCode { get; set; }
        public int NoOfBags { get; set; }
        public decimal Quantity { get; set; }
        public decimal RatePerKg { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal GstPercentage { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal TotalAmount { get; set; }
        public int? PackTypeId { get; set; }
        public int? UOMId { get; set; }
        public string? UOMName { get; set; }
    }
}
