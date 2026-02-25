namespace InventoryManagement.Application.Issue.Queries.GetPendingIssue
{
    public class GetPendingIssueDto
    {
        // Header fields
        public int MrsId { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int RequestCategoryId { get; set; }
        public string? RequestCategoryName { get; set; }
        public string? MrsNo { get; set; }
        public DateTimeOffset MrsDate { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int SubDepartmentId { get; set; }
        public string? SubDepartmentName { get; set; }
        public string? Remarks { get; set; }
        public int SubStoresWarehouseId { get; set; }
        public string? SubStoresWarehouseName { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public List<GetPendingIssueDetailsDto> PendingIssueDetails { get; set; } = new();

        public class GetPendingIssueDetailsDto
        {
            // Detail fields
            public int MrsDetailId { get; set; }
            public int MrsHeaderId { get; set; }
            public int ItemId { get; set; }
            public string? ItemName { get; set; }
            public string? ItemCode { get; set; }
            public int UomId { get; set; }
            public string? UomName { get; set; }
            public decimal RequestQuantity { get; set; }
            public int CostCenterId { get; set; }
            public string? FinanceCode { get; set; }
            public int WarehouseStockId { get; set; }
            public string? WarehouseStockName { get; set; }
            // Computed fields
            public decimal IssuedQuantity { get; set; }
            public decimal PendingQuantity { get; set; }
            public List<GetPendingStockBinDto> PendingStock { get; set; } = new();
        }
        public class GetPendingStockBinDto
        {
            public int ItemId { get; set; }              // ✅ Added
            public int WarehouseId { get; set; } 
            public int StorageTypeId { get; set; }
            public string? StorageTypeName { get; set; }
            public int TargetId { get; set; }
            public string? TargetCode { get; set; }
            public string? TargetName { get; set; }
            public int UomId { get; set; }
            public string? UomName { get; set; }
            public decimal CurrentStockQty { get; set; }
            public decimal CurrentStockValue { get; set; }
            public decimal AvgRate { get; set; }
            
        }
    }
}