namespace PurchaseManagement.Application.IssueReturn.Command.UpdateIssueReturn
{
    public class UpdateIssueReturnDto
    {
        public int Id { get; set; }
        public int RequestCategoryId { get; set; }
        public int UnitId { get; set; }
        public int? IssueHeaderId { get; set; }
        public int DepartmentId { get; set; }
        public string? Remarks { get; set; }
        public List<UpdateIssueReturnDetailDto> UpdateIssueReturnDetails { get; set; } = new();

        public class UpdateIssueReturnDetailDto
        {
            public int IssueReturnHeaderId { get; set; }
            public int ItemId { get; set; }
            public int UomId { get; set; }
            public int WarehouseStockId { get; set; }
            public int? StorageTypeId { get; set; }
            public int? TargetId { get; set; }
            public decimal ReturnQuantity { get; set; }
            public decimal ReturnValue { get; set; }
            public int ReasonId { get; set; }
            public string? Remarks { get; set; }
            public int? SubStoresDepartmentId { get; set; }
        }
    }
}