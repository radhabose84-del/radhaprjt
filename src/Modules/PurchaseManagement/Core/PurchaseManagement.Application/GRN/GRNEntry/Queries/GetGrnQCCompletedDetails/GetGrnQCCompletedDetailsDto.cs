namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnQCCompletedDetails
{
    public class GetGrnQCCompletedDetailsDto
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
        public string? QcRemarks { get; set; }
        public string? QcPersonName { get; set; }
        public DateTimeOffset? QcDate { get; set; }
        public int? QcWarehouseId { get; set; }
        public string? QcWarehouseName { get; set; }
        public bool? IsQcApproved { get; set; }
        public string? RejectedImage { get; set; }
       
    }
}