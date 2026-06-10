namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndentById
{
    public class PendingIndentDetailByIdDto
    {
        public int Id { get; set; }
        public int IndentHeaderId { get; set; }
        public int ApprovalRequestLineId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = default!;
        public int ItemCategoryId { get; set; }
        public string ItemCategoryName { get; set; } = default!;
        public int ItemUOMId { get; set; }
        public string UOMName { get; set; } = default!;
        public decimal QuantityRequired { get; set; }
        // Original quantity captured by the command repo when the approver edits the line during
        // approval. Null on creator-side edits and on lines the approver hasn't touched.
        public decimal? OldQuantityRequired { get; set; }
        public DateOnly RequiredDate { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public int PRConsumptionDays { get; set; }
        public string Remark { get; set; } = default!;
        public string Status { get; set; } = default!;
        public bool IsRFQDone { get; set; }
        
        
    }
}