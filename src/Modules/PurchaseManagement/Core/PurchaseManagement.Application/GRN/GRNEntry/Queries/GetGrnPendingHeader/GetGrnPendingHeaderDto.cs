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

        // QC.MiscMaster Id for QP_SOURCE_TYPE / code 'GRN' — resolved by the handler via IQcMiscMasterLookup.
        public int? SourceTypeId { get; set; }

        // GRN line items for this header row. Populated by the repo (qty/Po/Item Ids) and enriched
        // by the handler (ItemCode/ItemName via IItemLookup, IsTemplateAvailable via IQualitySpecificationLookup).
        public List<GrnPendingHeaderDetailDto> GrnDetails { get; set; } = new();

        public class GrnPendingHeaderDetailDto
        {
            public int Id { get; set; }
            public int? GrnDetailId { get; set; }
            public int PoId { get; set; }
            public int PoSlNoLocal { get; set; }
            public int ItemId { get; set; }
            public string? ItemCode { get; set; }
            public string? ItemName { get; set; }
            public decimal OrderQuantity { get; set; }
            public decimal DcQuantity { get; set; }
            public decimal ReceivedQuantity { get; set; }
            // Per-line QC sign-off (column lives on Purchase.GrnDetail).
            public bool IsQcApproved { get; set; }
            // "Y" if an active Qc.QualitySpecification exists for this ItemId or its ItemCategoryId.
            public string IsTemplateAvailable { get; set; } = "N";
        }
    }
}