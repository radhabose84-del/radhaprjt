namespace FinanceManagement.Application.EWaybillHeader.Dto
{
    public class EWaybillHeaderDto
    {
        public int Id { get; set; }
        public int? EInvoiceHeaderId { get; set; }
        public string? InvoiceNo { get; set; }            // From linked EInvoiceHeader (if any)
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? EWBNumber { get; set; }
        public DateOnly? InvoiceDate { get; set; }
        public decimal InvoiceValue { get; set; }
        public string? SupplyType { get; set; }
        public string? SubSupplyType { get; set; }
        public string? DocumentType { get; set; }
        public int? TransactionType { get; set; }
        public string? FromGSTIN { get; set; }
        public string? FromTradeName { get; set; }
        public string? ToGSTIN { get; set; }
        public string? ToTradeName { get; set; }
        public decimal TotalValue { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal Cess { get; set; }
        public int? TransporterId { get; set; }
        public string? TransporterName { get; set; }
        public string? TransporterGSTIN { get; set; }
        public string? TransportMode { get; set; }
        public string? TransDocNo { get; set; }
        public DateOnly? TransDocDate { get; set; }
        public string? VehicleNo { get; set; }
        public string? VehicleType { get; set; }
        public int? Distance { get; set; }
        public int? PartyId { get; set; }
        public string? PartyName { get; set; }
        public DateTimeOffset? GeneratedDate { get; set; }
        public DateTimeOffset? ValidUpto { get; set; }
        public string? EwbStatus { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTimeOffset? CancelledDate { get; set; }
        public string? CancelReason { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
