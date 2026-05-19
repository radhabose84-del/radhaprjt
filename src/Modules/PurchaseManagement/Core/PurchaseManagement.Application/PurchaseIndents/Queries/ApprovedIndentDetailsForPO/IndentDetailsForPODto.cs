namespace PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO
{
    public class IndentDetailsForPODto
    {
        public int Id { get; set; }
        public int IndentHeaderId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = default!;
        public int ItemCategoryId { get; set; }
        public string ItemCategoryName { get; set; } = default!;
        public int ItemUOMId { get; set; }
        public string UOMName { get; set; } = default!;
        public decimal QuantityRequired { get; set; }
        public DateOnly RequiredDate { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public int PRConsumptionDays { get; set; }
        public string Remark { get; set; } = default!;
        public decimal GSTPercentage { get; set; }
        public string HSNCode { get; set; } = default!;
        public decimal UnitPrice { get; set; }
        public decimal LastPOPrice { get; set; }
        public bool IsOnSpot { get; set; }
        public int? EmergencyPOById { get; set; }
        public decimal? EmergencyValueLimit { get; set; }
        public int? EmergencyActionId { get; set; }
    }
}