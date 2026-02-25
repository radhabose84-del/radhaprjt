namespace PurchaseManagement.Application.Issue.Command.CreateIssueEntry
{
    public class CreateIssueEntryDto
    {
        public int UnitId { get; set; }
        public int MrsHeaderId { get; set; }
        public int DepartmentId { get; set; }
        public int RequestCategoryId { get; set; } 
        public int? SubStoresWarehouseId { get; set; }
        public string? Remarks { get; set; }
        public List<CreateIssueDetailDto> IssueDetails { get; set; } = new();
        public class CreateIssueDetailDto
        {
            public int Sno { get; set; }
            public int ItemId { get; set; }
            public int UomId { get; set; }
            public decimal RequestQuantity { get; set; }
            public int WarehouseStockId { get; set; }
            public int StorageTypeId { get; set; }
            public int TargetId { get; set; }
            public decimal AvgRate { get; set; }
            public decimal? IssueQuantity { get; set; }
            public decimal? IssueValue { get; set; }
            public int? CostCenterId { get; set; }
            public int? FinanceCode { get; set; }
        
        }
    }
}