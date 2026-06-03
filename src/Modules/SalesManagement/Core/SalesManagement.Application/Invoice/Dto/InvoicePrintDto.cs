namespace SalesManagement.Application.Invoice.Dto
{
    public class InvoicePrintDto
    {
        public string? TripSheetNo { get; set; }
        public InvoicePrintCompanyDto? Company { get; set; }
        public InvoicePrintRegisteredOfficeDto? RegisteredOffice { get; set; }
        public InvoicePrintHeaderDto? Invoice { get; set; }
        public InvoicePrintTransportDto? Transport { get; set; }
        public InvoicePrintAgentDto? Agent { get; set; }
        public InvoicePrintPartyDto? BilledTo { get; set; }
        public InvoicePrintPartyDto? Consignee { get; set; }
        public List<InvoicePrintItemDto>? Items { get; set; }
        public InvoicePrintTotalsDto? Totals { get; set; }
        public InvoicePrintBankDto? Bank { get; set; }

        // Terms & Conditions HTML resolved from TnCTemplateMaster (Purchase module) by matching
        // the invoice's transaction type. Populated via ITnCTemplateLookup (cross-module).
        public string? TermsHtml { get; set; }
    }

    public class InvoicePrintCompanyDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Gstin { get; set; }
        public string? Pan { get; set; }
        public string? Cin { get; set; }
        public string? Email { get; set; }
        public string? Web { get; set; }
        public string? Phone { get; set; }
    }

    public class InvoicePrintRegisteredOfficeDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
    }

    public class InvoicePrintHeaderDto
    {
        public string? Type { get; set; }
        public string? SerialNumber { get; set; }
        public string? Date { get; set; }
        public string? CustomerPO { get; set; }
        public string? PaymentTerms { get; set; }
        public string? PlaceOfSupply { get; set; }
        public string? DateTimeOfSupply { get; set; }
        public string? InvoiceTypeFull { get; set; }
        public string? IrNo { get; set; }
        public string? AckNo { get; set; }
        public string? AckDate { get; set; }
        public string? EWayBillNo { get; set; }
        public string? EWayDate { get; set; }
        public string? ReverseCharge { get; set; }
    }

    public class InvoicePrintTransportDto
    {
        public string? TransporterName { get; set; }
        public string? VehicleNo { get; set; }
        public string? Phone { get; set; }
    }

    public class InvoicePrintAgentDto
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Phone { get; set; }
        public string? Pan { get; set; }
    }

    public class InvoicePrintPartyDto
    {
        public string? NameCode { get; set; }
        public string? Address { get; set; }
        public string? Street { get; set; }
        public string? Area { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? StateCode { get; set; }
        public string? Gstin { get; set; }
        public string? Pan { get; set; }
        public string? Phone { get; set; }
    }

    public class InvoicePrintItemDto
    {
        public int SNo { get; set; }
        public string? HsnCode { get; set; }
        public string? HsnGroup { get; set; }
        public string? Description { get; set; }
        public string? LotNo { get; set; }
        public string? BagSNo { get; set; }
        public decimal NoBags { get; set; }
        public decimal QuantityKg { get; set; }
        public decimal Rate { get; set; }
        public decimal Value { get; set; }
    }

    public class InvoicePrintTotalsDto
    {
        public int TotalBags { get; set; }
        public decimal TotalQtyKg { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalFreight { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal Insurance { get; set; }
        public decimal HandlingCharges { get; set; }
        public decimal TotalCharity { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal ValueOfSupply { get; set; }
        public decimal CgstRate { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstRate { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstRate { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TcsRate { get; set; }
        public decimal TcsAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string? InvoiceAmountWords { get; set; }
        public string? Remarks { get; set; }
    }

    public class InvoicePrintBankDto
    {
        public string? Name { get; set; }
        public string? Branch { get; set; }
        public string? Address { get; set; }
        public string? AccountNo { get; set; }
        public string? Ifsc { get; set; }
    }

    // --- Raw Dapper mapping DTOs (used by repository for SQL result mapping) ---

    public class PrintHeaderRawDto
    {
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public string? InvoiceTypeName { get; set; }
        public int DispatchAdviceId { get; set; }
        public int PartyId { get; set; }
        public int? AgentId { get; set; }
        public int UnitId { get; set; }
        public int? TransportMode { get; set; }
        public string? TransportModeName { get; set; }
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
        public DateTimeOffset? CreatedDate { get; set; }
    }

    public class PrintDetailRawDto
    {
        public int ItemSno { get; set; }
        public int ItemId { get; set; }
        public string? HsnCode { get; set; }
        public decimal NoOfBags { get; set; }
        public decimal BagWeight { get; set; }
        public decimal NetWeight { get; set; }
        public decimal RatePerKg { get; set; }
        public decimal TaxableAmount { get; set; }
        public int? LotId { get; set; }
        public decimal CgstPercentage { get; set; }
        public decimal SgstPercentage { get; set; }
        public decimal IgstPercentage { get; set; }
    }

    public class PrintDispatchRawDto
    {
        public int SalesOrderId { get; set; }
        public int DispatchAddressId { get; set; }
        public int? TransporterId { get; set; }
    }

    public class PrintBagRawDto
    {
        public int ItemId { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
    }

    public class PrintConsigneeRawDto
    {
        public string? DispatchAddressName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public string? PinCode { get; set; }
        public string? ContactPerson { get; set; }
        public string? MobileNumber { get; set; }
        public string? GSTIN { get; set; }
    }

}
