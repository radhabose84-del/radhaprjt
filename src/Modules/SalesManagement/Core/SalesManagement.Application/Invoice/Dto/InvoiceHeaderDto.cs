namespace SalesManagement.Application.Invoice.Dto
{
    public class InvoiceHeaderDto
    {
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int InvoiceType { get; set; }
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
        public int? TransportMode { get; set; }
        public string? TransportModeName { get; set; }
        public string? VehicleNumber { get; set; }
        public string? TransporterName { get; set; }
        public string? LRNumber { get; set; }
        public DateOnly? LRDate { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal Discount { get; set; }
        public decimal Freight { get; set; }
        public decimal Insurance { get; set; }
        public decimal HandlingCharge { get; set; }
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
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public List<InvoiceDetailDto>? InvoiceDetails { get; set; }
    }
}
