namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnQCCompletedDetails
{
    public class GetGrnQCCompletedDto
    {
        public int GrnId { get; set; }
        public string? GrnNo { get; set; }
        public DateTimeOffset? GrnDate { get; set; }
        public int UnitId { get; set; }
        public int GateEntryId { get; set; }
        public string? GateEntryNo { get; set; }
        public DateTimeOffset? GateEntryDate { get; set; }
        public int PartyId { get; set; }
        public string? QcRemarks { get; set; }
        public int? QcStatusId { get; set; }
        public string? QcPersonName { get; set; }
        public DateTimeOffset? QcDate { get; set; }
        public int? QcWarehouseId { get; set; }
        public bool? IsQcApproved { get; set; }
        public List<GetGrnQCCompletedDtoDetails> GrnDetails { get; set; } = new();
        public class GetGrnQCCompletedDtoDetails
        {
           // public int Id { get; set; }
            public int? GrnDetailId { get; set; }
            public int PoId { get; set; }
            public int PoSlNoLocal { get; set; }
            public string? PoNumber { get; set; }
            public int ItemId { get; set; }
            public string? ItemCode { get; set; }
            public string? ItemName { get; set; }
            public string? UOMName { get; set; }
            public decimal? QcAcceptedQuantity { get; set; }
            public decimal? UnitPrice { get; set; }
            public string? GrnDetailImage { get; set; }
            public List<PutawayRuleDto>? PutawayRules { get; set; }

            public class PutawayRuleDto
            {
                public int PutAwayRuleId { get; set; }
                public int StorageTypeId { get; set; }
                public string? StorageTypeName { get; set; }
                public int TargetId { get; set; }
                public string? TargetCode { get; set; }
                public string? TargetName { get; set; }
                public int PriorityId { get; set; }
                public string PriorityName { get; set; } = "";
                public int? WarehouseId { get; set; }
                public string? WarehouseCode { get; set; }
                public string? WarehouseName { get; set; }
                public int ItemId { get; set; }
                public string ItemCode { get; set; } = string.Empty; // ✅ Added for clarity
                public string? ItemCategoryName { get; set; }
                public string? ItemGroupName { get; set; }
                public int? StockUomId { get; set; }
                public string? StockUom { get; set; }
                public int? PurchaseUomId { get; set; }
                public string? PurchaseUom { get; set; }
                public string? ItemName { get; set; }
                public double? ConversionRate { get; set; }
                  // ✅ New calculated quantity field
                public decimal? CalculatedPutawayQty { get; set; }

            }
        }
    }
}
    
