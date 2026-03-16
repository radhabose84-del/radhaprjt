namespace FinanceManagement.Application.EInvoiceHeader.Dto
{
    public class EInvoiceHeaderDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? DocType { get; set; }
        public string? SupplyType { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public string? PlaceOfSupply { get; set; }
        public string? IrnNumber { get; set; }
        public string? AckNo { get; set; }
        public DateTimeOffset? AckDate { get; set; }
        public string? SignInvoice { get; set; }
        public string? SignQrCode { get; set; }
        public string? IrnStatus { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public string? GstNo { get; set; }
        public bool ReverseCharge { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal Cess { get; set; }
        public decimal StateCess { get; set; }
        public decimal TCS { get; set; }
        public decimal Discount { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal RoundOff { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string? Remarks { get; set; }
        public int? StatusId { get; set; }
        public bool EWaybillCreated { get; set; }
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

        public List<EInvoiceDetailDto> Details { get; set; } = new();
    }
}
