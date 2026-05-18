namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingDetails
{
    public class GetGrnPendingDetailsDto
    {
        public int GrnId { get; set; }
        public string? GrnNo { get; set; }
        public DateTimeOffset? GrnDate { get; set; }
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
        public string? Remarks { get; set; }
        public bool IsGrnGenerated { get; set; }
        public string? GrnReceivedImage { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? QcRemarks { get; set; }
        public int? QcStatusId { get; set; }
        public string? QcPersonName { get; set; }
        public DateTimeOffset? QcDate { get; set; }
        public int? QcWarehouseId { get; set; }
        public bool? IsQcApproved { get; set; }
        public string? RejectedImage { get; set; }
        // Details 
        public List<GetGateEntryPendingDetailsGRNDto> GrnDetails { get; set; } = new();
        public class GetGateEntryPendingDetailsGRNDto
        {
            public int Id { get; set; }
            public int? GrnDetailId { get; set; }
            public int PoId { get; set; }
            public int PoSlNoLocal { get; set; }
            public int PoCategoryId { get; set; }
            public int PoMethodId { get; set; }
            public string? PoNumber { get; set; }
            public int ItemId { get; set; }
            public string? ItemCode { get; set; }
            public string? ItemName { get; set; }
            public string? UOMName { get; set; }
            public decimal OrderQuantity { get; set; }
            public decimal DcQuantity { get; set; }
            public decimal UpperTolerance { get; set; }
            public decimal LowerTolerance { get; set; }
            public decimal ReceivedQuantity { get; set; }
            public DateTimeOffset? ExpiryDate { get; set; }
            public string? BatchNumber { get; set; }
            // QC fields 
            public decimal? QcAcceptedQuantity { get; set; }
            public decimal? QcRejectedQuantity { get; set; }
            public string? QcRejectedRemarks { get; set; }
            public string? GrnDetailImage { get; set; }

            // Calculated
            public decimal PendingQty { get; set; } }
        }
}