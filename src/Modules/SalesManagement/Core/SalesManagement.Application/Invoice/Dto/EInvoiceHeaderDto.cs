namespace SalesManagement.Application.Invoice.Dto
{
    public class EInvoiceHeaderDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public string? IrnNumber { get; set; }
        public string? AckNo { get; set; }
        public DateTimeOffset? AckDate { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal TCS { get; set; }
        public decimal RoundOff { get; set; }
        public decimal InvoiceAmount { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public string? GstNo { get; set; }
        public decimal Discount { get; set; }
        public decimal Cess { get; set; }
        public decimal OtherCharges { get; set; }
        public bool ReverseCharge { get; set; }
        public string? SignInvoice { get; set; }
        public string? SignQrCode { get; set; }
        public string? EwbNo { get; set; }
        public DateTimeOffset? EwbDate { get; set; }
        public DateTimeOffset? EwbValidTill { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public List<EInvoiceDetailDto>? EInvoiceDetails { get; set; }
    }
}
