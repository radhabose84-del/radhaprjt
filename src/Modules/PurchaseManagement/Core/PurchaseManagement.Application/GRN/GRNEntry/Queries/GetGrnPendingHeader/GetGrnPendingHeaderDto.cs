namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingHeader
{
    public class GetGrnPendingHeaderDto
    {
        public int GrnId { get; set; }
        public string? GrnNo { get; set; }
        public DateTimeOffset? GrnDate { get; set; }
        //public int GrnEntryId { get; set; }
        public int UnitId { get; set; }
        public int GateEntryId { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public string? GateEntryNo { get; set; }
        public DateTimeOffset? GateEntryDate { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTimeOffset? InvoiceDate { get; set; }
        public string? DcNo { get; set; }
        public DateTimeOffset? DcDate { get; set; }
        public int? ReceivingWarehouseId { get; set; }
        public string? ReceivingWarehouseName { get; set; }
        public string? Remarks { get; set; }
        public bool IsGrnGenerated { get; set; }
        public string? GrnReceivedImage { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        // QC display fields (QcRemarks, QcStatusId, QcPersonName, QcDate) moved to per-line.
        public int? QcWarehouseId { get; set; }
        public string? QcWarehouseName { get; set; }
        // Computed AND aggregate: true only when every detail line is QC-approved.
        public bool? IsQcApproved { get; set; }
        public string? RejectedImage { get; set; }
       
    }
}