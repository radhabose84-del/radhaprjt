using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;

namespace PurchaseManagement.Domain.Entities.GRN.GRNEntry
{
    public class GrnHeader
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? GrnNo { get; set; }
        public DateTimeOffset GrnDate { get; set; }
        public int GateEntryId { get; set; }
        public GateEntryHeader GrnHeaderDetails { get; set; } = null!;
        public int PartyId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTimeOffset InvoiceDate { get; set; }
        public string? DcNo { get; set; }
        public DateTimeOffset? DcDate { get; set; }
        public int ReceivingWarehouseId { get; set; }
        public string? Remarks { get; set; }
        public bool IsGrnGenerated { get; set; }
        public string? GrnReceivedImage { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
        public string? QcRemarks { get; set; }
        public string? QcPersonName { get; set; }
        public int? QcStatusId { get; set; }
        public MiscMaster? GrnQcStatus { get; set; }
        public DateTimeOffset? QcDate { get; set; }
        public string? QcApprovedIp { get; set; }
        public bool IsQcApproved { get; set; }
        public int? QcWarehouseId { get; set; }
        public string? RejectedImage { get; set; }
        public decimal? ItemsTotal { get; set; }
        public decimal? DiscountTotal { get; set; }
        public decimal? TaxableAmount { get; set; }
        public decimal? CGSTTotal { get; set; }
        public decimal? SGSTTotal { get; set; }
        public decimal? IGSTTotal { get; set; }
        public decimal? MiscCharges { get; set; }
        public decimal? RoundOff { get; set; }
        public decimal? PurchaseValue { get; set; }
        public ICollection<GrnDetail>? GrnDetails { get; set; }
    }
}