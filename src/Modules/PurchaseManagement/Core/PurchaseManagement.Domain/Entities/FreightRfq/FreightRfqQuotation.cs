using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.FreightRfq;

/// <summary>
/// A single transporter quotation captured against a Freight RFQ. One RFQ can hold many rows;
/// exactly one may be marked <see cref="IsSelected"/>. <see cref="FreightValue"/> is system-calculated
/// from the rate basis. <see cref="IsOverride"/> is set when the selected row is not the lowest quote.
/// </summary>
public class FreightRfqQuotation : BaseEntity
{
    public int FreightRfqHeaderId { get; set; }
    public FreightRfqHeader Header { get; set; } = null!;

    public int TransporterId { get; set; }                 // cross-module Party (transporter), no DB FK
    public int? TransportDetailId { get; set; }            // Party transportDetails.id (selected vehicle/transport config)
    public int? RateBasisId { get; set; }                  // FK Purchase.MiscMaster -> Per Bale / Per MT / Per Vehicle (set when quote entered)
    public decimal? QuotedRate { get; set; }               // null until the transporter's reply is entered
    public int? NoOfVehicles { get; set; }                 // required when rate basis = Per Vehicle
    public decimal? FreightValue { get; set; }             // system-calculated once a rate is entered

    // Transport-detail snapshot (populated from Party at selection — shown on the comparison page)
    public string? VehicleNo { get; set; }
    public string? TransportModeName { get; set; }
    public string? VehicleTypeName { get; set; }

    public DateTimeOffset? NotifiedDate { get; set; }      // when the RFQ email was sent to this transporter

    public bool IsSelected { get; set; }                   // only one true per header (R5)
    public bool IsOverride { get; set; }                   // 1 when selected row != lowest quote
    public string? Remarks { get; set; }                   // per-row / override reason
}
