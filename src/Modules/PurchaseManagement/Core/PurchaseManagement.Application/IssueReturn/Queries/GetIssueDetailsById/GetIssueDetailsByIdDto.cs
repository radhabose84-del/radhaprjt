namespace PurchaseManagement.Application.IssueReturn.Queries.GetIssueDetailsById
{
    public class GetIssueDetailsByIdDto
    {
        public int IssueHeaderId { get; set; }
        public DateTimeOffset IssueDate { get; set; }
        public int MrsHeaderId { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int RequestCategoryId { get; set; }
        public string? RequestCategoryName { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int SubDepartmentId { get; set; }
        public string? SubDepartmentName { get; set; }
    
        public List<GetIssueDetailsByIssueIdDto> PendingIssueDetailsByIssueId { get; set; } = new();
        public class GetIssueDetailsByIssueIdDto
        {
            public int Sno { get; set; }
            public int ItemId { get; set; }
            public string? ItemName { get; set; }
            public string? ItemCode { get; set; }
            public int UomId { get; set; }
            public string? UomName { get; set; }
            public int WarehouseStockId { get; set; }
            public string? WarehouseStockName { get; set; }
            public int StorageTypeId { get; set; }
            public string? StorageTypeName { get; set; }
            public int TargetId { get; set; }
            public string? TargetCode { get; set; }
            public string? TargetName { get; set; }
            public decimal TotalIssueQuantity { get; set; }
            public decimal TotalIssueValue { get; set; }
            public decimal AlreadyReturnQuantity { get; set; }
            public decimal AlreadyReturnValue { get; set; }
            public decimal BalanceQuantity { get; set; }
            public decimal BalanceIssueValue { get; set; }
        }
    }
}