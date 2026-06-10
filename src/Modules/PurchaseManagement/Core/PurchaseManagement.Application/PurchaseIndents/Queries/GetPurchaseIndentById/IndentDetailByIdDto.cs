using Contracts.Dtos.Inventory;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentById
{
    public class IndentDetailByIdDto
    {
        public int Id { get; set; }
        public int IndentHeaderId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int ItemUOMId { get; set; }
        public string? ItemUOM { get; set; }
        public int ItemCategoryId { get; set; }
        public string? ItemCategory { get; set; }
        public decimal QuantityRequired { get; set; }
        // Original quantity at submit time — populated by the command repo only when the
        // approver edits the line during approval. Null otherwise.
        public decimal? OldQuantityRequired { get; set; }
        public DateOnly RequiredDate { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public int PRConsumptionDays { get; set; }
        public string? Remark { get; set; }
        public decimal GSTPercentage { get; set; }
        public string? HSNCode { get; set; }
        public string? Status { get; set; }
        public bool IsRFQDone { get; set; }
        public int SourceId { get; set; }
        public bool IsOnSpot { get; set; }
        public List<ItemVendorDto>? Vendors { get; set; }

    }
}