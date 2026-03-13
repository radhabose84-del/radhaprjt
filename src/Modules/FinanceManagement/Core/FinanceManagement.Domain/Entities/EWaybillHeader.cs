using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class EWaybillHeader : BaseEntity
    {
        public int? EInvoiceHeaderId { get; set; }        // NULL for standalone EWaybill; FK → Finance.EInvoiceHeader
        public int UnitId { get; set; }                   // Cross-module FK → UserManagement.Unit (from plant)
        public string? EWBNumber { get; set; }            // E-Waybill number from NIC portal
        public string? InvoiceNo { get; set; }
        public DateOnly? InvoiceDate { get; set; }
        public decimal InvoiceValue { get; set; }
        public string? SupplyType { get; set; }           // Outward / Inward
        public string? SubSupplyType { get; set; }        // Supply / Export / Job Work / etc.
        public string? DocumentType { get; set; }         // Tax Invoice / Bill of Supply / Bill of Entry / Others
        public int? TransactionType { get; set; }         // 1=Regular, 2=Bill To–Ship To, 3=Bill From–Dispatch From, 4=Combination
        public string? FromGSTIN { get; set; }
        public string? FromTradeName { get; set; }
        public string? ToGSTIN { get; set; }
        public string? ToTradeName { get; set; }
        public decimal TotalValue { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal Cess { get; set; }
        public int? TransporterId { get; set; }           // Cross-module FK → PartyManagement
        public string? TransporterGSTIN { get; set; }
        public string? TransporterName { get; set; }
        public string? TransportMode { get; set; }        // 1=Road, 2=Rail, 3=Air, 4=Ship
        public string? TransDocNo { get; set; }
        public DateOnly? TransDocDate { get; set; }
        public string? VehicleNo { get; set; }
        public string? VehicleType { get; set; }          // R=Regular, O=ODC
        public int? Distance { get; set; }
        public int? PartyId { get; set; }                 // Cross-module FK → PartyManagement (buyer/receiver)
        public DateTimeOffset? GeneratedDate { get; set; }
        public DateTimeOffset? ValidUpto { get; set; }
        public string? EwbStatus { get; set; }            // Pending / Generated / Cancelled / Expired
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTimeOffset? CancelledDate { get; set; }
        public string? CancelReason { get; set; }

        // Navigation properties
        public EInvoiceHeader? EInvoiceHeader { get; set; }
        public ICollection<EWaybillDetail>? EWaybillDetails { get; set; }
    }
}
