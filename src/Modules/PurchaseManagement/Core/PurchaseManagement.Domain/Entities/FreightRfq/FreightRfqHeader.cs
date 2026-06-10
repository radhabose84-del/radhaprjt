using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.FreightRfq;

/// <summary>
/// Freight RFQ header — a request for transporter quotations against a route, optionally
/// linked to a Purchase Order. Holds the route, quantities, the selected transporter, and
/// the self-contained approval state (Draft / Pending Approval / Approved / Rejected).
/// </summary>
public class FreightRfqHeader : BaseEntity, IActivityTracked
{
    // Identity
    public string FreightRfqNumber { get; set; } = string.Empty;   // System-generated via TransactionType (Id 59) document sequence, immutable
    public DateTimeOffset RfqDate { get; set; }                     // System date on create
    public DateTimeOffset? RfqValidTill { get; set; }              // RFQ expiry date (optional)

    // Type & PO linkage
    public int RfqTypeId { get; set; }                             // FK Purchase.MiscMaster -> "PO Based" / "Non-PO Based"
    public int? PoReferenceId { get; set; }                        // FK Purchase.RawMaterialPOHeader (only when PO Based)
    public int? SupplierId { get; set; }                          // From PO -> OCR SupplierId (cross-module Party, no DB FK)

    // Route — Source prefilled from PO -> OCR (Location/Station); Destination is user input
    public string SourceLocation { get; set; } = string.Empty;
    public string SourceStation { get; set; } = string.Empty;
    public string DestinationLocation { get; set; } = string.Empty;
    public string DestinationStation { get; set; } = string.Empty;

    // Freight calculation basis
    public decimal TotalQuantity { get; set; }                     // MT (prefilled = sum of RM PO detail Weight / 1000, editable)
    public int TotalBaleCount { get; set; }                        // Prefilled = sum of RM PO detail Quantity (bales)

    // Approval workflow (self-contained)
    public int StatusId { get; set; }                             // FK Purchase.MiscMaster -> Draft/Pending Approval/Approved/Rejected
    public int? SelectedQuotationId { get; set; }                 // FK to the chosen FreightRfqQuotation row
    public string? ComparisonRemarks { get; set; }                // "Reason for selection"

    // Approval snapshot (set on approve)
    public int? ApprovedTransporterId { get; set; }               // cross-module Party, no DB FK
    public decimal? ApprovedRate { get; set; }
    public decimal? ApprovedFreightValue { get; set; }
    public string? ApprovalRemarks { get; set; }

    // Children
    public ICollection<FreightRfqQuotation> Quotations { get; set; } = new List<FreightRfqQuotation>();
}
