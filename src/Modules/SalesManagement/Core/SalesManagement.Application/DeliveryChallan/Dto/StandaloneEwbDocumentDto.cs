namespace SalesManagement.Application.DeliveryChallan.Dto
{
    // Print-ready view of a standalone EWB (DC-based, no IRN).
    // Mirrors the company's e-Invoice tax document layout but with EWB-specific fields.
    public sealed class StandaloneEwbDocumentDto
    {
        // ── Document classification ────────────────────────────────────────
        public string? DocumentType { get; set; }            // "Delivery Challan"
        public string? DocumentNumber { get; set; }          // STODC/2025/0016
        public DateOnly? DocumentDate { get; set; }
        public string? SupplyType { get; set; }              // Outward / Inward
        public string? SubSupplyType { get; set; }           // For Own Use / Supply / etc.
        public int? TransactionType { get; set; }            // 1=Regular, 2=Bill To, ...

        // ── Company header (from FROM plant's company) ─────────────────────
        public string? CompanyName { get; set; }
        public string? CompanyGstNumber { get; set; }
        public string? CompanyPAN { get; set; }
        public string? CompanyCIN { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? CompanyPhone { get; set; }
        public string? RegisteredOfficeAddress { get; set; }

        // ── e-Waybill identifiers ──────────────────────────────────────────
        public string? EwbNumber { get; set; }
        public DateTimeOffset? EwbGeneratedDate { get; set; }
        public DateTimeOffset? EwbValidUpto { get; set; }
        public string? EwbStatus { get; set; }               // Generated / Pending / Cancelled

        // ── Consignor (From plant) ─────────────────────────────────────────
        public string? ConsignorName { get; set; }
        public string? ConsignorGstin { get; set; }
        public string? ConsignorAddressLine1 { get; set; }
        public string? ConsignorAddressLine2 { get; set; }
        public string? ConsignorCity { get; set; }
        public string? ConsignorState { get; set; }
        public int? ConsignorStateCode { get; set; }
        public int? ConsignorPincode { get; set; }
        public string? ConsignorPhone { get; set; }

        // ── Consignee (To plant) ───────────────────────────────────────────
        public string? ConsigneeName { get; set; }
        public string? ConsigneeGstin { get; set; }
        public string? ConsigneeAddressLine1 { get; set; }
        public string? ConsigneeAddressLine2 { get; set; }
        public string? ConsigneeCity { get; set; }
        public string? ConsigneeState { get; set; }
        public int? ConsigneeStateCode { get; set; }
        public int? ConsigneePincode { get; set; }
        public string? ConsigneePhone { get; set; }

        // ── Transporter ────────────────────────────────────────────────────
        public string? TransporterName { get; set; }
        public string? TransporterGstin { get; set; }
        public string? VehicleNumber { get; set; }
        public string? VehicleType { get; set; }             // R = Regular, O = ODC
        public string? TransportMode { get; set; }           // 1=Road, 2=Rail, 3=Air, 4=Ship
        public int? TransportDistance { get; set; }          // km
        public string? TransDocNo { get; set; }
        public DateOnly? TransDocDate { get; set; }

        // ── Line items + totals ────────────────────────────────────────────
        public List<StandaloneEwbDocumentItemDto> Items { get; set; } = new();
        public decimal TotalQuantity { get; set; }
        public decimal TotalTaxableValue { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal Cess { get; set; }
        public decimal InvoiceTotal { get; set; }
        public string? InvoiceTotalInWords { get; set; }

        // ── Audit ──────────────────────────────────────────────────────────
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
    }

    public sealed class StandaloneEwbDocumentItemDto
    {
        public int Sno { get; set; }
        public string? ItemName { get; set; }
        public string? Description { get; set; }
        public string? HsnCode { get; set; }
        public string? LotNumber { get; set; }
        public int? BagCount { get; set; }
        public string? PackRange { get; set; }              // "75339-75346, 75567-75596"
        public decimal Quantity { get; set; }
        public string? Uom { get; set; }
        public decimal Rate { get; set; }                    // ExMillRate
        public decimal NetWeight { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
    }
}
