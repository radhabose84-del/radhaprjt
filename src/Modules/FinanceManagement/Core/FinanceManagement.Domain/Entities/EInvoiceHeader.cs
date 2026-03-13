using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class EInvoiceHeader : BaseEntity
    {
        public int UnitId { get; set; }
        public string? DocType { get; set; }              // INV / CRN / DBN
        public string? SupplyType { get; set; }           // B2B / B2C / SEZWP / SEZWOP / EXPWP / EXPWOP
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public string? PlaceOfSupply { get; set; }        // State code 01–38
        public string? IrnNumber { get; set; }
        public string? AckNo { get; set; }
        public DateTimeOffset? AckDate { get; set; }
        public string? SignInvoice { get; set; }
        public string? SignQrCode { get; set; }
        public string? IrnStatus { get; set; }            // Pending / Generated / Failed / Cancelled
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public int PartyId { get; set; }                  // Cross-module FK → PartyManagement
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
        public int? StatusId { get; set; }                // Cross-module FK — MiscMaster status
        public bool EWaybillCreated { get; set; }         // Default = false

        // Child collections
        public ICollection<EInvoiceDetail>? EInvoiceDetails { get; set; }
        public ICollection<EWaybillHeader>? EWaybillHeaders { get; set; }
    }
}
