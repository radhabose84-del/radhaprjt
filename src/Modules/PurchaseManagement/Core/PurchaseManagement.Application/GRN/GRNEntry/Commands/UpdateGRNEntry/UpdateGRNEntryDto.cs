namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry
{
    public class UpdateGRNEntryDto
    {
        public int Id { get; set; }
        public int GateEntryId { get; set; }
        public int PartyId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTimeOffset InvoiceDate { get; set; }
        public string? DcNo { get; set; }
        public DateTimeOffset? DcDate { get; set; }
        public int ReceivingWarehouseId { get; set; }
        public string? Remarks { get; set; }
        public byte IsGrnGenerated { get; set; }
        // QC fields moved to per-line on UpdateGRNDetailsDto.
        // Header retains only the warehouse + rejected image (no QC sign-off here).
        public int? QcWarehouseId { get; set; }
        public string? RejectedImage { get; set; }
        public List<UpdateGRNDetailsDto>? UpdateGRNDetailsDtos { get; set; }

        public class UpdateGRNDetailsDto
        {
            public int Id { get; set; }
            public int GrnId { get; set; }
            public int PoId { get; set; }
            public int? PoSlNoLocal { get; set; }
            public int PoCategoryId { get; set; }
            public int PoMethodId { get; set; }
            public int ItemId { get; set; }
            public decimal OrderQuantity { get; set; }
            public decimal DcQuantity { get; set; }
            public decimal? UpperTolerance { get; set; }
            public decimal? LowerTolerance { get; set; }
            public decimal ReceivedQuantity { get; set; }
            public DateTimeOffset? ExpiryDate { get; set; }
            public string? BatchNumber { get; set; }
            public decimal? QcAcceptedQuantity { get; set; }
            public decimal? QcRejectedQuantity { get; set; }
            public string? QcRejectedRemarks { get; set; }
            public string? GrnDetailImage { get; set; }

            // Per-line QC sign-off (moved from header).
            public string? QcPersonName { get; set; }
            public string? QcRemarks { get; set; }
            public int? QcStatusId { get; set; }
            public DateTimeOffset? QcDate { get; set; }
            public string? QcApprovedIp { get; set; }
            public byte IsQcApproved { get; set; }
        }
    }
}
