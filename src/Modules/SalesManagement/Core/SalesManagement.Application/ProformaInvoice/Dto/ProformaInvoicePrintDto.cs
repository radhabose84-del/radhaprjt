namespace SalesManagement.Application.ProformaInvoice.Dto
{
    public class ProformaInvoicePrintDto
    {
        public ProformaInvoicePrintCompanyDto? Company { get; set; }
        public ProformaInvoicePrintRegisteredOfficeDto? RegisteredOffice { get; set; }
        public ProformaInvoicePrintHeaderDto? Header { get; set; }
        public ProformaInvoicePrintPartyDto? BilledTo { get; set; }
        public ProformaInvoicePrintAgentDto? Agent { get; set; }
        public List<ProformaInvoicePrintItemDto>? Items { get; set; }
        public ProformaInvoicePrintTotalsDto? Totals { get; set; }
        public ProformaInvoicePrintBankDto? Bank { get; set; }
    }

    public class ProformaInvoicePrintCompanyDto
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

    public class ProformaInvoicePrintRegisteredOfficeDto
    {
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    public class ProformaInvoicePrintHeaderDto
    {
        public string? PiNumber { get; set; }
        public string? PiDate { get; set; }
        public string? SalesOrderNo { get; set; }
        public string? SalesOrderDate { get; set; }
    }

    public class ProformaInvoicePrintAgentDto
    {
        public string? Name { get; set; }
        public string? Gstin { get; set; }
    }

    public class ProformaInvoicePrintPartyDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? StateCode { get; set; }
        public string? Gstin { get; set; }
        public string? Pan { get; set; }
        public string? Phone { get; set; }
    }

    public class ProformaInvoicePrintItemDto
    {
        public int SNo { get; set; }
        public string? ProductCategory { get; set; }
        public string? ProductName { get; set; }
        public string? HsnGroup { get; set; }
        public string? HsnCode { get; set; }
        public int NoPacks { get; set; }
        public decimal QuantityKg { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }

    public class ProformaInvoicePrintTotalsDto
    {
        public int TotalPacks { get; set; }
        public decimal TotalQtyKg { get; set; }
        public decimal TotalItemValue { get; set; }
        public decimal Discount { get; set; }
        public decimal Freight { get; set; }
        public decimal Insurance { get; set; }
        public decimal HandlingCharges { get; set; }
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

    public class ProformaInvoicePrintBankDto
    {
        public string? BankName { get; set; }
        public string? AccountNo { get; set; }
        public string? IfscCode { get; set; }
        public string? Branch { get; set; }
    }

    // --- Raw Dapper mapping DTOs (used internally by the repository) ---

    public class ProformaPrintHeaderRawDto
    {
        public int Id { get; set; }
        public string? ProformaNumber { get; set; }
        public DateOnly ProformaDate { get; set; }
        public int SalesOrderId { get; set; }
        public string? SalesOrderNo { get; set; }
        public DateOnly SalesOrderDate { get; set; }
        public int UnitId { get; set; }
        public int PartyId { get; set; }
        public int? AgentId { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TotalFreight { get; set; }
        public decimal GSTPercentage { get; set; }
        public decimal TotalGST { get; set; }
        public decimal TCSPercentage { get; set; }
        public decimal TotalTCS { get; set; }
        public decimal FinalAmount { get; set; }
        public string? Remarks { get; set; }
    }

    public class ProformaPrintItemRawDto
    {
        public int SNo { get; set; }
        public int ItemId { get; set; }
        public int HSNId { get; set; }
        public int NoOfPacks { get; set; }
        public decimal QuantityKg { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxPercentage { get; set; }
    }
}
